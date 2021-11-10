using System;
using System.Collections.Generic;
using UnityEngine;

public class FootPath
{
    private const int _keyframes = 5;
    private const int _lastKeyframe = _keyframes - 1;

    public float duration => _times[_lastKeyframe];

    private readonly float[] _times = new float[_keyframes];
    private readonly Vector3[] _positions = new Vector3[_keyframes];
    private readonly Quaternion[] _yaws = new Quaternion[_keyframes];
    private readonly float[] _pitches = new float[_keyframes];
    private readonly float[] _pitchWeight = new float[_keyframes];

    public Vector3 GetPositionAtIndex(int i)
    {
        return _positions[i];
    }

    public Vector3 EvaluatePosition(float t)
    {
        // TODO: Use a better curve instead of this
        return Evaluate(t, _positions, Vector3.Lerp);
    }

    public Quaternion EvaluateYaw(float t)
    {
        return Evaluate(t, _yaws, Quaternion.Slerp);
    }

    public float EvaluatePitch(float t)
    {
        return Evaluate(t, _pitches, Mathf.Lerp);
    }

    public float EvaluatePitchWeight(float t)
    {
        return Evaluate(t, _pitchWeight, Mathf.Lerp);
    }

    public void Set(int i, float t, Vector3 position, Quaternion yaw, float pitch, float pitchWeight)
    {
        _times[i] = t;
        _positions[i] = position;
        _yaws[i] = yaw;
        _pitches[i] = pitch;
        _pitchWeight[i] = pitchWeight;
    }

    private T Evaluate<T>(float t, IList<T> values, Func<T, T, float, T> lerp)
    {
        for (var i = 1; i < _keyframes; i++)
        {
            if (t < _times[i])
                return lerp(values[i - 1], values[i], (t - _times[i - 1]) / (_times[i] - _times[i - 1]));
        }

        return values[_lastKeyframe];
    }
}
