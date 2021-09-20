using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Walk : MVRScript
{
    private readonly List<GameObject> _walkComponents = new List<GameObject>();

    public override void Init()
    {
        if (containingAtom == null || containingAtom.type != "Person")
        {
            SuperController.LogError($"Walk: Can only apply on person atoms. Was assigned on a '{containingAtom.type}' atom named '{containingAtom.uid}'.");
            enabled = false;
            return;
        }

        var style = new GaitStyle();

        style.SetupStorables(this);

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

        var lFootStateVisualizer = AddWalkComponent<FootStateVisualizer>("LeftFootStateVisualizer", c => { }, false);

        var lFootController = AddWalkComponent<FootController>("LeftFoot", c => c.Configure(
            style,
            new GaitFootStyle(style, -1),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lFootControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lKneeControl"),
            lFootStateVisualizer
        ), false);

        var rFootStateVisualizer = AddWalkComponent<FootStateVisualizer>("RightFootStateVisualizer", c => { }, false);

        var rFootController = AddWalkComponent<FootController>("RightFoot", c => c.Configure(
            style,
            new GaitFootStyle(style, 1),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rFootControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rKneeControl"),
            rFootStateVisualizer
        ), false);

        var heading = AddWalkComponent<HeadingTracker>("HeadingTracker", c => c.Configure(
            style,
            containingAtom.rigidbodies.FirstOrDefault(fc => fc.name == "head"),
            bones.FirstOrDefault(fc => fc.name == "head")
        ));

        var gaitVisualizer = AddWalkComponent<GaitVisualizer>("BodyPostureVisualizer", c => c.Configure(
            containingAtom.rigidbodies.FirstOrDefault(rb => rb.name == "hip")
        ));

        var gait = AddWalkComponent<GaitController>("Gait", c => c.Configure(
            heading,
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

        // TODO: Separate the moving state (feet close, forward) and the standing state (separate feet, rotate out)
        var movingState = AddWalkComponent<MovingState>("MovingState", c => c.Configure(
            style,
            heading,
            gait,
            movingStateVisualizer
        ), false);

        var teleportState = AddWalkComponent<TeleportState>("TeleportState", c => c.Configure(
            gait,
            heading
        ), false);

        AddWalkComponent<StateMachine>("StateMachine", c => c.Configure(
            idleState,
            movingState,
            teleportState
        ));
    }

    public void OnEnable()
    {
        foreach(var c in _walkComponents)
            c.SetActive(true);
    }

    public void OnDisable()
    {
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
        go.transform.SetParent(transform, false);
        var c = go.AddComponent<T>();
        configure(c);
        if (active) go.SetActive(true);
        return c;
    }
}
