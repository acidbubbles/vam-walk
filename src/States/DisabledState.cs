using System;
using UnityEngine;

public class DisabledState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }
    public MonoBehaviour visualizer => null;

    private WalkConfiguration _config;
    private GaitController _gait;

    public void Configure(WalkConfiguration config, GaitController gait)
    {
        _config = config;
        _gait = gait;
    }

    public void Update()
    {
        if (_config.allowWalk.val)
        {
            stateMachine.currentState = stateMachine.idleState;
            return;
        }
    }

    public void OnEnable()
    {
        _gait.gameObject.SetActive(false);
        _gait.lFoot.gameObject.SetActive(false);
        _gait.rFoot.gameObject.SetActive(false);
    }

    public void OnDisable()
    {
        _gait.lFoot.gameObject.SetActive(true);
        _gait.rFoot.gameObject.SetActive(true);
        _gait.gameObject.SetActive(true);
    }
}
