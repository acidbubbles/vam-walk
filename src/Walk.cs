public class Walk : MVRScript
{
    private WalkContext _context;

    public override void Init()
    {
        if (containingAtom == null || containingAtom.type != "Person")
        {
            SuperController.LogError($"Walk: Can only apply on person atoms. Was assigned on a '{containingAtom.type}' atom named '{containingAtom.uid}'.");
            enabled = false;
            return;
        }
        _context = new WalkContext(this);
    }

    public void Update()
    {
        _context.Update();
    }
}
