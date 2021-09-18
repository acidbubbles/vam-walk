using System;
using System.Collections.Generic;
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
        var context = AddWalkComponent<BalanceContext>("Walk_LeftFoot", c => c.Configure(this));
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

    private T AddWalkComponent<T>(string goName, Action<T> configure) where T : MonoBehaviour
    {
        var go = new GameObject(goName);
        go.SetActive(false);
        _walkComponents.Add(go);
        go.transform.SetParent(transform, false);
        var c = go.AddComponent<T>();
        configure(c);
        go.SetActive(true);
        return c;
    }
}
