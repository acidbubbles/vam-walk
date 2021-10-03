using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Walk : MVRScript
{
    private readonly List<GameObject> _walkComponents = new List<GameObject>();
    private readonly List<GameObject> _defaultActiveComponents = new List<GameObject>();
    private StateMachine _stateMachine;

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
        var personMeasurements = new PersonMeasurements(bones, style);

        // TODO: Wait for model loaded
        personMeasurements.Sync();

        var lFootStateVisualizer = AddWalkComponent<FootStateVisualizer>("LeftFootControllerVisualizer", c => { }, false);

        var lFootController = AddWalkComponent<FootController>("LeftFootController", c => c.Configure(
            style,
            new GaitFootStyle(style, -1),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lFootControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lKneeControl"),
            lFootStateVisualizer
        ), false);

        var rFootStateVisualizer = AddWalkComponent<FootStateVisualizer>("RightFootControllerVisualizer", c => { }, false);

        var rFootController = AddWalkComponent<FootController>("RightFootController", c => c.Configure(
            style,
            new GaitFootStyle(style, 1),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rFootControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rKneeControl"),
            rFootStateVisualizer
        ), false);

        var heading = AddWalkComponent<HeadingTracker>("HeadingTracker", c => c.Configure(
            style,
            personMeasurements,
            containingAtom.rigidbodies.FirstOrDefault(fc => fc.name == "head"),
            bones.FirstOrDefault(fc => fc.name == "head")
        ));

        var gaitVisualizer = AddWalkComponent<GaitVisualizer>("GaitVisualizer", c => c.Configure(
            containingAtom.rigidbodies.FirstOrDefault(rb => rb.name == "hip")
        ));

        var gait = AddWalkComponent<GaitController>("GaitController", c => c.Configure(
            heading,
            personMeasurements,
            lFootController,
            rFootController,
            style,
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "hipControl"),
            gaitVisualizer
        ));

        var idleStateVisualizer = AddWalkComponent<IdleStateVisualizer>("IdleStateVisualizer", c => { }, false);

        var idleState = AddWalkComponent<IdleState>("IdleState", c => c.Configure(
            style,
            gait,
            heading,
            idleStateVisualizer
        ), false);

        var movingStateVisualizer = AddWalkComponent<MovingStateVisualizer>("MovingStateVisualizer", c => { }, false);

        var movingState = AddWalkComponent<WalkingState>("WalkingState", c => c.Configure(
            style,
            heading,
            gait,
            movingStateVisualizer
        ), false);

        var teleportState = AddWalkComponent<JumpingState>("JumpingState", c => c.Configure(
            gait,
            heading
        ), false);

        _stateMachine = AddWalkComponent<StateMachine>("StateMachine", c => c.Configure(
            idleState,
            movingState,
            teleportState
        ));
    }

    private static void InitUI(UI ui, GaitStyle style)
    {
        style.SetupUI(ui);
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
