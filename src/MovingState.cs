public class MovingState : IWalkState
{
    private readonly WalkContext _context;

    public MovingState(WalkContext context)
    {
        _context = context;
    }

    public void Update()
    {
        // When stabilized
        _context.currentState = _context.idleState;
    }
}
