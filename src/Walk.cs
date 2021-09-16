public class Walk : MVRScript
{
    private WalkContext _context;

    public override void Init()
    {
        _context = new WalkContext(this);
    }

    public void Update()
    {
        _context.currentState.Update();
    }
}
