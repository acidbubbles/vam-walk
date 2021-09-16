public class IdleState : IWalkState
{
    private readonly WalkContext _context;

    public IdleState(WalkContext context)
    {
        _context = context;
    }

    public void Update()
    {
        if (!_context.IsBalanced())
        {
            _context.currentState = _context.movingState;
        }
    }
}
