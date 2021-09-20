using UnityEngine;

public interface IWalkState
{
    StateMachine stateMachine { get; set; }
    GameObject gameObject { get; }
}
