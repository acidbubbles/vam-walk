using UnityEngine;

public class GaitController : MonoBehaviour
{
    private WalkContext _context;
    private GaitVisualizer _visualizer;

    public FootState currentFoot { get; private set; }
    public FootState lFoot => _context.lFootState;
    public FootState rFoot => _context.rFootState;

    public void Configure(WalkContext context, GaitVisualizer visualizer)
    {
        _context = context;
        _visualizer = visualizer;
    }

    public void SelectClosestFoot(Vector3 position)
    {
        var weightCenter = _context.GetBodyCenter();
        currentFoot = lFoot.position.PlanarDistance(weightCenter) > rFoot.position.PlanarDistance(weightCenter)
            ? lFoot
            : rFoot;
    }

    public void SelectOtherFoot()
    {
        currentFoot = currentFoot == lFoot ? rFoot : lFoot;
    }

    public void OnEnable()
    {
        _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }
}
