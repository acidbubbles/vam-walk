public class Walk : MVRScript
{
    private BalanceContext _context;

    public override void Init()
    {
        if (containingAtom == null || containingAtom.type != "Person")
        {
            SuperController.LogError($"Walk: Can only apply on person atoms. Was assigned on a '{containingAtom.type}' atom named '{containingAtom.uid}'.");
            enabled = false;
            return;
        }
        _context = new BalanceContext(this);
    }

    public void FixedUpdate()
    {
        _context.FixedUpdate();
    }
}
