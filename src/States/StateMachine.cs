﻿using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public IWalkState currentState
    {
        get { return _currentState; }
        set
        {
            _currentState?.gameObject.SetActive(false);
            _currentState = value;
            _currentState.gameObject.SetActive(true);
        }
    }

    public IWalkState idleState { get; private set; }
    public IWalkState walkingState { get; private set; }
    public IWalkState jumpingState { get; private set; }

    private IWalkState _currentState;

    public void Configure(IWalkState idleState, IWalkState walkingState, IWalkState jumpingState)
    {
        idleState.stateMachine = this;
        this.idleState = idleState;

        walkingState.stateMachine = this;
        this.walkingState = walkingState;

        jumpingState.stateMachine = this;
        this.jumpingState = jumpingState;
    }

    public void Awake()
    {
        currentState = idleState;
    }
}
