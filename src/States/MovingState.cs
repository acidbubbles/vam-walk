﻿using UnityEngine;

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
        var feetCenter = _gait.GetFloorFeetCenter();

        if (Vector3.Distance(_heading.GetFloorDesiredCenter(), feetCenter) > _style.stepLength.val * 2)
        {
            stateMachine.currentState = stateMachine.teleportState;
            return;
        }

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
        var toRotation = _heading.GetPlanarRotation();

        var position = foot.GetFootPositionRelativeToBodyWalking(GetProjectedPosition(bodyCenter),  toRotation);
        position = Vector3.MoveTowards(
            foot.floorPosition,
            position,
            _style.stepLength.val
        );

        var rotation = foot.GetFootRotationRelativeToBodyWalking(toRotation);

        foot.PlotCourse(position, rotation);
    }

    private Vector3 GetProjectedPosition(Vector3 headingCenter)
    {
        var velocity = _heading.GetPlanarVelocity();
        // TODO: Make this an option, how much of the velocity is used for prediction
        var finalPosition = headingCenter + velocity * (_style.stepDuration.val * 0.9f);
        return finalPosition;
    }
}
