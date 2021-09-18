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

        var style = new WalkStyle
        {
            footRightOffset = 0.09f,
            footUpOffset = 0.05f,
        };

        var context = AddWalkComponent<WalkContext>("Context", c => c.Configure(
            this
        ));

        var lFoot = AddWalkComponent<FootState>("LeftFoot", c => c.Configure(
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "lFootControl"),
            new FootConfig(style, -1).Sync()
        ));
        var rFoot = AddWalkComponent<FootState>("RightFoot", c => c.Configure(
            containingAtom.freeControllers.FirstOrDefault(fc => fc.name == "rFootControl"),
            new FootConfig(style, 1).Sync()
        ));

        var idleState = AddWalkComponent<IdleState>("IdleState", c => c.Configure(
            context
        ), false);

        var movingState = AddWalkComponent<MovingState>("MovingState", c => c.Configure(
            context,
            lFoot,
            rFoot
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
