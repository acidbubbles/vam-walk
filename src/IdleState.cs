using UnityEngine;

public class IdleState : IWalkState
{
    private readonly BalanceContext _context;

    public IdleState(BalanceContext context)
    {
        _context = context;
    }

    public void Enter()
    {
    }

    public void Update()
    {
        if (!IsBalanced())
        {
            _context.currentState = _context.movingState;
            return;
        }

        // TODO: Small movements, hips roll, in-place feet movements
    }

    private bool IsBalanced()
    {
        // TODO: We should also check if forward has a 60 degrees angle from the feet line, and if so it's not balanced either.
        return _context.GetFeetCenter().PlanarDistance(_context.GetWeightCenter()) < _context.GetFeetCenterRadius();
    }

    public void Leave()
    {
    }
}
