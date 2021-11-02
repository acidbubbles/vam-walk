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

        var style = new GaitStyle();
        style.RegisterStorables(this);

        var ui = new UI(this);
        InitUI(ui, style);

        try
        {
            SetupDependencyTree(style);
        }
        catch (Exception exc)
        {
            gameObject.SetActive(false);
            SuperController.LogError($"Walk: {exc}");
        }
    }

    private void SetupDependencyTree(GaitStyle style)
    {
        var bones = containingAtom.transform.Find("rescale2").GetComponentsInChildren<DAZBone>();

        // TODO: Refresh when style.footFloorDistance changes or when the model changes
        _personMeasurements = new PersonMeasurements(bones, style);

        // TODO: Wait for model loaded
        _personMeasurements.Sync();

        var measurementsVisualizer = AddWalkComponent<MeasurementsVisualizer>(nameof(MeasurementsVisualizer), c => c.Configure(
            style,
            _personMeasurements
        ), false);

        var lFootStateVisualizer = AddWalkComponent<FootStateVisualizer>(nameof(FootStateVisualizer), c => c.Configure(
            style
        ), false);

        var lFootController = AddWalkComponent<FootController>(nameof(FootController), c => c.Configure(
            style,
            new GaitFootStyle(style, -1),
            bones.FirstOrDefault(fc => fc.name == "lFoot"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lFootControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lKneeControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lToeControl"),
            new HashSet<Collider>(bones.First(b => b.name == "rThigh").GetComponentsInChildren<Collider>()),
            lFootStateVisualizer
        ));

        var rFootStateVisualizer = AddWalkComponent<FootStateVisualizer>(nameof(FootStateVisualizer), c => c.Configure(
            style
        ), false);

        var rFootController = AddWalkComponent<FootController>(nameof(FootController), c => c.Configure(
            style,
            new GaitFootStyle(style, 1),
            bones.FirstOrDefault(fc => fc.name == "rFoot"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rFootControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rKneeControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rToeControl"),
            new HashSet<Collider>(bones.First(b => b.name == "lThigh").GetComponentsInChildren<Collider>()),
            rFootStateVisualizer
        ));

        var heading = AddWalkComponent<HeadingTracker>(nameof(HeadingTracker), c => c.Configure(
            style,
            _personMeasurements,
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "headControl"),
            containingAtom.rigidbodies.FirstOrDefault(fc => fc.name == "head"),
            bones.FirstOrDefault(fc => fc.name == "head")
        ));

        var gaitVisualizer = AddWalkComponent<GaitVisualizer>(nameof(GaitVisualizer), c => c.Configure(
            containingAtom.rigidbodies.FirstOrDefault(rb => rb.name == "hip")
        ), style.visualizersEnabled.val);

        var gait = AddWalkComponent<GaitController>(nameof(GaitController), c => c.Configure(
            style,
            heading,
            _personMeasurements,
            lFootController,
            rFootController,
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "hipControl"),
            gaitVisualizer
        ));

        var idleStateVisualizer = AddWalkComponent<IdleStateVisualizer>(nameof(IdleStateVisualizer), c => { }, false);

        var idleState = AddWalkComponent<IdleState>(nameof(IdleState), c => c.Configure(
            style,
            gait,
            heading,
            idleStateVisualizer
        ), false);

        var walkingStateVisualizer = AddWalkComponent<WalkingStateVisualizer>(nameof(WalkingStateVisualizer), c => { }, false);

        var walkingState = AddWalkComponent<WalkingState>(nameof(WalkingState), c => c.Configure(
            style,
            heading,
            gait,
            walkingStateVisualizer
        ), false);

        var jumpingStateVisualizer = AddWalkComponent<JumpingStateVisualizer>(nameof(JumpingStateVisualizer), c => { }, false);

        var jumpingState = AddWalkComponent<JumpingState>(nameof(JumpingState), c => c.Configure(
            style,
            gait,
            heading,
            jumpingStateVisualizer
        ), false);

        _stateMachine = AddWalkComponent<StateMachine>(nameof(StateMachine), c => c.Configure(
            idleState,
            walkingState,
            jumpingState
        ));

        style.visualizersEnabledChanged.AddListener(val =>
        {
            measurementsVisualizer.gameObject.SetActive(val);
            gaitVisualizer.gameObject.SetActive(val);
            _stateMachine.currentState.visualizer.gameObject.SetActive(val);
        });
    }

    private void InitUI(UI ui, GaitStyle style)
    {
        ui.AddHeader("Control", 1);
        ui.AddAction("Refresh Measurements", false, () =>
        {
            _personMeasurements.Sync();
        });
        ui.AddAction("Optimize Controls", false, () =>
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

        ui.AddHeader("Profiles", 1);
        ui.AddAction("Import Profile", false, () =>
        {
            SuperController.LogMessage("Profiles are not yet implemented");
        });
        ui.AddAction("Export Profile", false, () =>
        {
            SuperController.LogMessage("Profiles are not yet implemented");
        });

        style.SetupUI(ui);
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
