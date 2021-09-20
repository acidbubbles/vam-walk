using UnityEngine;

public class MovingState : MonoBehaviour, IWalkState
{
    public StateMachine stateMachine { get; set; }

    private GaitStyle _style;
    private HeadingTracker _heading;
    private GaitController _gait;
    private MovingStateVisualizer _visualizer;


    public void Configure(GaitStyle style, HeadingTracker heading, GaitController gait, MovingStateVisualizer visualizer)
    {
        _style = style;
        _heading = heading;
        _gait = gait;
        _visualizer = visualizer;
    }

    public void OnEnable()
    {
        var bodyCenter = _heading.GetFloorCenter();
        _gait.SelectClosestFoot(bodyCenter);
        PlotFootCourse(bodyCenter);
        _visualizer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        _visualizer.gameObject.SetActive(false);
    }

    public void Update()
    {
        var bodyCenter = _heading.GetFloorCenter();
        _visualizer.Sync(bodyCenter, GetProjectedPosition(bodyCenter));

        if (!_gait.currentFoot.FloorContact()) return;

        if (FeetAreStable(bodyCenter))
        {
            // TODO: If the feet distance is too far away, move to another state that'll do instant catchup
            stateMachine.currentState = stateMachine.idleState;
            return;
        }

        _gait.SelectOtherFoot();
        PlotFootCourse(bodyCenter);
    }

    private bool FeetAreStable(Vector3 bodyCenter)
    {
        var projectedPosition = GetProjectedPosition(bodyCenter);
        var bodyRotation = _heading.GetPlanarRotation();
        var lFootDistance = Vector3.Distance(_gait.lFoot.position, _gait.lFoot.GetFootPositionRelativeToBodyWalking(projectedPosition, bodyRotation));
        const float footDistanceEpsilon = 0.01f;
        if(lFootDistance > footDistanceEpsilon) return false;
        var rFootDistance = Vector3.Distance(_gait.rFoot.position, _gait.rFoot.GetFootPositionRelativeToBodyWalking(projectedPosition, bodyRotation));
        if(rFootDistance > footDistanceEpsilon) return false;
        return true;
    }

    private void PlotFootCourse(Vector3 bodyCenter)
    {
        var foot = _gait.currentFoot;

        var position = foot.GetFootPositionRelativeToBodyWalking(GetProjectedPosition(bodyCenter), _heading.GetPlanarRotation());
        position = Vector3.MoveTowards(
            foot.floorPosition,
            position,
            _style.stepLength.val
        );

        var rotation = Quaternion.LookRotation(_heading.GetBodyForward(), Vector3.up);

        _gait.currentFoot.PlotCourse(position, rotation);
    }

    private Vector3 GetProjectedPosition(Vector3 headingCenter)
    {
        var velocity = _heading.GetPlanarVelocity();
        // TODO: 0.5f is the step time, 0.8f is how much of this time should be predict
        var finalPosition = headingCenter + velocity * (0.7f * 0.6f);
        return finalPosition;
    }
}
