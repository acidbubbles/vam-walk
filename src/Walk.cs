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

        var style = new WalkStyle();

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

    private void SetupDependencyTree(WalkStyle style)
    {
        var lFootStateVisualizer = AddWalkComponent<FootStateVisualizer>("LeftFootStateVisualizer", c => { }, false);

        var lFootState = AddWalkComponent<FootState>("LeftFoot", c => c.Configure(
            style,
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lFootControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lKneeControl"),
            new FootConfig(style, -1),
            lFootStateVisualizer
        ), false);

        var rFootStateVisualizer = AddWalkComponent<FootStateVisualizer>("RightFootStateVisualizer", c => { }, false);

        var rFootState = AddWalkComponent<FootState>("RightFoot", c => c.Configure(
            style,
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rFootControl"),
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rKneeControl"),
            new FootConfig(style, 1),
            rFootStateVisualizer
        ), false);

        var context = AddWalkComponent<WalkContext>("Context", c => c.Configure(
            this,
            lFootState,
            rFootState
        ));

        var gaitVisualizer = AddWalkComponent<GaitVisualizer>("BodyPostureVisualizer", c => c.Configure(
            containingAtom.rigidbodies.FirstOrDefault(rb => rb.name == "hip")
        ));

        var gait = AddWalkComponent<GaitController>("Gait", c => c.Configure(
            context,
            gaitVisualizer
        ));

        var idleStateVisualizer = AddWalkComponent<IdleStateVisualizer>("IdleStateVisualizer", c => { }, false);

        var idleState = AddWalkComponent<IdleState>("IdleState", c => c.Configure(
            style,
            context,
            idleStateVisualizer
        ), false);

        var movingStateVisualizer = AddWalkComponent<MovingStateVisualizer>("MovingStateVisualizer", c => { }, false);

        // TODO: Separate the moving state (feet close, forward) and the standing state (separate feet, rotate out)
        var movingState = AddWalkComponent<MovingState>("MovingState", c => c.Configure(
            style,
            context,
            gait,
            movingStateVisualizer
        ), false);

        AddWalkComponent<StateMachine>("StateMachine", c => c.Configure(
            idleState,
            movingState
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
