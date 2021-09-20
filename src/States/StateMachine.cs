using UnityEngine;

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
    public IWalkState movingState { get; private set; }
    public IWalkState teleportState { get; private set; }

    private IWalkState _currentState;

    public void Configure(IWalkState idleState, IWalkState movingState, IWalkState teleportState)
    {
        idleState.stateMachine = this;
        this.idleState = idleState;

        movingState.stateMachine = this;
        this.movingState = movingState;

        teleportState.stateMachine = this;
        this.teleportState = teleportState;
    }

    public void Awake()
    {
        currentState = idleState;
    }
}
