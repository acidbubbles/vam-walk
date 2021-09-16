public class WalkContext
{
    public IWalkState currentState { get; set; }

    public readonly IdleState idleState;
    public readonly MovingState movingState;

    public WalkContext(MVRScript plugin)
    {
        idleState = new IdleState(this);
        movingState = new MovingState(this);

        currentState = idleState;
    }

    public bool IsBalanced()
    {
        return true;
    }
}
