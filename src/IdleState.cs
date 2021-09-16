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
        if (IsOffBalanceDistance() || IsOffBalanceRotation())
        {
            _context.currentState = _context.movingState;
            return;
        }

        // TODO: Small movements, hips roll, in-place feet movements
    }

    private bool IsOffBalanceDistance()
    {
        // TODO: We should also check if forward has a 60 degrees angle from the feet line, and if so it's not balanced either.
        return _context.GetFeetCenter().PlanarDistance(_context.GetBodyCenter()) > _context.GetFeetCenterRadius();
    }

    private bool IsOffBalanceRotation()
    {
        // TODO: Configure this
        return Vector3.Angle(_context.GetFeetForward(), _context.GetBodyForward()) > 50;
    }

    public void Leave()
    {
    }
}
