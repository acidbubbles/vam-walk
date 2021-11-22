using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Walk : MVRScript
{
    private readonly List<GameObject> _walkComponents = new List<GameObject>();
    private readonly List<GameObject> _defaultActiveComponents = new List<GameObject>();
    private StateMachine _stateMachine;
    private PersonMeasurements _personMeasurements;

    public override void Init()
    {
        if (containingAtom == null || containingAtom.type != "Person")
        {
            SuperController.LogError($"Walk: Can only apply on person atoms. Was assigned on a '{containingAtom.type}' atom named '{containingAtom.uid}'.");
            enabled = false;
            return;
        }

        var config = new WalkConfiguration();
        config.RegisterStorables(this);

        var ui = new UI(this);
        InitUI(ui, config);

        try
        {
            SetupDependencyTree(config);
        }
        catch (Exception exc)
        {
            gameObject.SetActive(false);
            SuperController.LogError($"Walk: {exc}");
        }
    }

    private void SetupDependencyTree(WalkConfiguration config)
    {
        var bones = containingAtom.transform.Find("rescale2").GetComponentsInChildren<DAZBone>();

        // TODO: Refresh when style.footFloorDistance changes or when the model changes
        _personMeasurements = new PersonMeasurements(bones, config);

        // TODO: Wait for model loaded
        _personMeasurements.Sync();

        #if(VIZ_MEASUREMENTS)
        var measurementsVisualizer = AddWalkComponent<MeasurementsVisualizer>(nameof(MeasurementsVisualizer), c => c.Configure(
            style,
            _personMeasurements
        ), false);
        #endif

        var lFootStateVisualizer = AddWalkComponent<FootStateVisualizer>(nameof(FootStateVisualizer), c => c.Configure(
            config
        ), false);

        var lFootController = AddWalkComponent<FootController>(nameof(FootController), c => c.Configure(
            config,
            new FootConfiguration(config, -1, cfg => cfg.lFootTargetAtom != null ? cfg.lFootTargetAtom.freeControllers[0] : null),
            bones.FirstOrDefault(fc => fc.name == "lFoot"),
            bones.FirstOrDefault(fc => fc.name == "lToe"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lFootControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lKneeControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lToeControl"),
            new HashSet<Collider>(bones.First(b => b.name == "rThigh").GetComponentsInChildren<Collider>()),
            lFootStateVisualizer
        ));

        var rFootStateVisualizer = AddWalkComponent<FootStateVisualizer>(nameof(FootStateVisualizer), c => c.Configure(
            config
        ), false);

        var rFootController = AddWalkComponent<FootController>(nameof(FootController), c => c.Configure(
            config,
            new FootConfiguration(config, 1, cfg => cfg.rFootTargetAtom != null ? cfg.rFootTargetAtom.freeControllers[0] : null),
            bones.FirstOrDefault(fc => fc.name == "rFoot"),
            bones.FirstOrDefault(fc => fc.name == "rToe"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rFootControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rKneeControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rToeControl"),
            new HashSet<Collider>(bones.First(b => b.name == "lThigh").GetComponentsInChildren<Collider>()),
            rFootStateVisualizer
        ));

        var heading = AddWalkComponent<HeadingTracker>(nameof(HeadingTracker), c => c.Configure(
            config,
            _personMeasurements,
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "headControl"),
            containingAtom.rigidbodies.FirstOrDefault(fc => fc.name == "head"),
            bones.FirstOrDefault(fc => fc.name == "head")
        ));

        var gaitVisualizer = AddWalkComponent<GaitVisualizer>(nameof(GaitVisualizer), c => c.Configure(
            containingAtom.rigidbodies.FirstOrDefault(rb => rb.name == "hip")
        ), config.visualizersEnabled.val);

        var gait = AddWalkComponent<GaitController>(nameof(GaitController), c => c.Configure(
            config,
            heading,
            _personMeasurements,
            lFootController,
            rFootController,
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "hipControl"),
            gaitVisualizer
        ));

        var disabledState = AddWalkComponent<DisabledState>(nameof(DisabledState), c => c.Configure(
            config,
            gait
        ), false);

        var idleStateVisualizer = AddWalkComponent<IdleStateVisualizer>(nameof(IdleStateVisualizer), c => { }, false);

        var idleState = AddWalkComponent<IdleState>(nameof(IdleState), c => c.Configure(
            config,
            gait,
            heading,
            idleStateVisualizer
        ), false);

        var walkingStateVisualizer = AddWalkComponent<WalkingStateVisualizer>(nameof(WalkingStateVisualizer), c => { }, false);

        var walkingState = AddWalkComponent<WalkingState>(nameof(WalkingState), c => c.Configure(
            config,
            heading,
            gait,
            walkingStateVisualizer
        ), false);

        var jumpingStateVisualizer = AddWalkComponent<JumpingStateVisualizer>(nameof(JumpingStateVisualizer), c => { }, false);

        var jumpingState = AddWalkComponent<JumpingState>(nameof(JumpingState), c => c.Configure(
            config,
            gait,
            heading,
            jumpingStateVisualizer
        ), false);

        _stateMachine = AddWalkComponent<StateMachine>(nameof(StateMachine), c => c.Configure(
            disabledState,
            idleState,
            walkingState,
            jumpingState
        ));

        config.visualizersEnabledChanged.AddListener(val =>
        {
            #if(VIZ_MEASUREMENTS)
            measurementsVisualizer.gameObject.SetActive(val);
            #endif
            gaitVisualizer.gameObject.SetActive(val);
            lFootStateVisualizer.gameObject.SetActive(val);
            rFootStateVisualizer.gameObject.SetActive(val);
            _stateMachine.currentState.visualizer.gameObject.SetActive(val);
        });
    }

    private void InitUI(UI ui, WalkConfiguration config)
    {
        ui.AddHeader("Control", 1);
        ui.AddAction("Refresh Measurements", false, () =>
        {
            _personMeasurements.Sync();
        });
        ui.AddAction("Configure Controls for Possession", false, () =>
        {
            foreach (var fc in containingAtom.freeControllers.Where(fc => fc.name != "control"))
            {
                fc.currentPositionState = FreeControllerV3.PositionState.Off;
                fc.currentRotationState = FreeControllerV3.RotationState.Off;
            }

            SetControlOptions("headControl");
            SetControlOptions("lHandControl");
            SetControlOptions("rHandControl");
            SetControlOptions("hipControl");
            SetControlOptions("lFootControl");
            SetControlOptions("rFootControl");
        });
        ui.AddAction("Optimize Head Height", false, () =>
        {
            var headControl = containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "headControl");
            if (headControl == null) throw new NullReferenceException($"Could not find headControl");
            var headPosition = headControl.control.position;
            headPosition.y = _personMeasurements.floorToHead;
            headControl.control.position = headPosition;
        });
        ui.AddAction("Hands Off", false, () =>
        {
            var lHandControl = containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lHandControl");
            if (lHandControl == null) throw new NullReferenceException($"Could not find lHandControl");
            lHandControl.currentPositionState = FreeControllerV3.PositionState.Off;
            lHandControl.currentRotationState = FreeControllerV3.RotationState.Off;

            var rHandControl = containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rHandControl");
            if (rHandControl == null) throw new NullReferenceException($"Could not find rHandControl");
            rHandControl.currentPositionState = FreeControllerV3.PositionState.Off;
            rHandControl.currentRotationState = FreeControllerV3.RotationState.Off;
        });
        ui.AddAction("Force Walk", false, () =>
        {
            _stateMachine.currentState = _stateMachine.walkingState;
        });

        ui.AddHeader("Profiles", 1);
        ui.AddAction("Import Profile", false, () =>
        {
            SuperController.LogMessage("Profiles are not yet implemented");
        });
        ui.AddAction("Export Profile", false, () =>
        {
            SuperController.LogMessage("Profiles are not yet implemented");
        });

        ui.AddHeader("Behavior", 1);
        ui.AddBool(config.allowWalk);
        ui.AddStringChooser(config.lFootTarget);
        ui.AddStringChooser(config.rFootTarget);

        ui.AddHeader("Debugging", 1);
        ui.AddBool(config.visualizersEnabled);

        ui.AddHeader("Foot Position", 1);
        ui.AddFloat(config.footFloorDistance);
        ui.AddFloat(config.footBackOffset);
        ui.AddFloat(config.footPitch);
        ui.AddFloat(config.footRoll);

        ui.AddHeader("While Standing", 2);
        ui.AddFloat(config.footStandingOutOffset);
        ui.AddFloat(config.footStandingYaw);

        ui.AddHeader("While Walking", 2);
        ui.AddFloat(config.footWalkingOutOffset);
        ui.AddFloat(config.footWalkingYaw);

        ui.AddHeader("Step Configuration", 1);
        ui.AddFloat(config.stepDuration);
        ui.AddFloat(config.stepDistance);
        ui.AddFloat(config.stepHeight);
        ui.AddFloat(config.minStepHeightRatio);
        ui.AddFloat(config.kneeForwardForce);
        ui.AddFloat(config.passingDistance);

        ui.AddHeader("Prediction", 1);
        ui.AddFloat(config.predictionStrength);

        ui.AddHeader("Late Acceleration", 1);
        ui.AddFloat(config.lateAccelerateSpeedToStepRatio);
        ui.AddFloat(config.lateAccelerateMaxSpeed);

        ui.AddHeader("Animation Curve", 1, true);

        ui.AddHeader("Timing", 2, true);
        ui.AddFloat(config.toeOffTimeRatio, true);
        ui.AddFloat(config.midSwingTimeRatio, true);
        ui.AddFloat(config.heelStrikeTimeRatio, true);

        ui.AddHeader("Height", 2, true);
        ui.AddFloat(config.toeOffHeightRatio, true);

        ui.AddHeader("Distance", 2, true);
        ui.AddFloat(config.toeOffDistanceRatio, true);
        ui.AddFloat(config.midSwingDistanceRatio, true);
        ui.AddFloat(config.heelStrikeDistanceRatio, true);

        ui.AddHeader("Pitch", 2, true);
        ui.AddFloat(config.toeOffPitch, true);
        ui.AddFloat(config.midSwingPitch, true);
        ui.AddFloat(config.heelStrikePitch, true);

        ui.AddHeader("Hip", 1, true);

        ui.AddHeader("While Standing", 2, true);
        ui.AddFloat(config.hipStandingForward, true);
        ui.AddFloat(config.hipStandingPitch, true);

        ui.AddHeader("While Crouching", 2, true);
        ui.AddFloat(config.hipCrouchingUp, true);
        ui.AddFloat(config.hipCrouchingForward, true);
        ui.AddFloat(config.hipCrouchingPitch, true);

        ui.AddHeader("While Walking", 2, true);
        ui.AddFloat(config.hipStepSide, true);
        ui.AddFloat(config.hipStepRaise, true);
        ui.AddFloat(config.hipStepYaw, true);
        ui.AddFloat(config.hipStepRoll, true);

        ui.AddHeader("Misc", 1, true);
        ui.AddFloat(config.jumpTriggerDistance, true);
    }

    private void SetControlOptions(string controlName)
    {
        var control = containingAtom.freeControllers.FirstOrDefault(fc => fc.name == controlName);
        if (control == null) throw new NullReferenceException($"Could not find control {controlName}");
        control.currentPositionState = FreeControllerV3.PositionState.On;
        control.currentRotationState = FreeControllerV3.RotationState.On;
        control.RBHoldPositionSpring = 10000f;
        control.RBHoldRotationSpring = 1000f;
    }

    public void OnEnable()
    {
        if (_stateMachine == null) return;

        foreach(var c in _defaultActiveComponents)
            c.SetActive(true);

        _stateMachine.currentState = _stateMachine.idleState;
    }

    public void OnDisable()
    {
        if (_stateMachine != null)
            _stateMachine.currentState = _stateMachine.idleState;

        foreach(var c in _walkComponents)
            c.SetActive(false);
    }

    public void OnDestroy()
    {
        CustomPrefabs.Destroy();
        foreach(var c in _walkComponents)
            Destroy(c);
    }

    private T AddWalkComponent<T>(string goName, Action<T> configure, bool active = true) where T : MonoBehaviour
    {
        var go = new GameObject($"Walk_{goName}");
        go.SetActive(false);
        _walkComponents.Add(go);
        if (active) _defaultActiveComponents.Add(go);
        go.transform.SetParent(transform, false);
        var c = go.AddComponent<T>();
        configure(c);
        if (active) go.SetActive(true);
        return c;
    }
}
