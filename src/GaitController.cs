using UnityEngine;

public class GaitController : MonoBehaviour
{
    private WalkContext _context;
    private GaitVisualizer _visualizer;

    public FootState currentFootState { get; private set; }
    public FootState lFootState => _context.lFootState;
    public FootState rFootState => _context.rFootState;

    public void Configure(WalkContext context, GaitVisualizer visualizer)
    {
        _context = context;
        _visualizer = visualizer;
    }

    public void SelectClosestFoot(Vector3 position)
    {
        var weightCenter = _context.GetBodyCenter();
        currentFootState = lFootState.position.PlanarDistance(weightCenter) > rFootState.position.PlanarDistance(weightCenter)
            ? lFootState
            : rFootState;
    }

    public void SelectOtherFoot()
    {
        currentFootState = currentFootState == lFootState ? rFootState : lFootState;
    }

    public void PlotFootCourse(Vector3 position, Quaternion rotation)
    {
        currentFootState.PlotCourse(position, rotation);
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
