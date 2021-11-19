using System;
using System.Collections.Generic;
using UnityEngine;

public class FootAnimationCurve<T>
{
    private readonly Func<T, T, float, T> _interpolate;
    private readonly float[] _times;
    private readonly T[] _values;

    public FootAnimationCurve(int count, Func<T, T, float, T> interpolate)
    {
        _times = new float[count];
        _values = new T[count];
        _interpolate = interpolate;
    }

    public void Set(int i, float t, T value)
    {
        _times[i] = t;
        _values[i] = value;
    }

    public T Evaluate(float t)
    {
        for (var i = 1; i < _values.Length; i++)
        {
            if (t < _times[i])
                return _interpolate(_values[i - 1], _values[i], (t - _times[i - 1]) / (_times[i] - _times[i - 1]));
        }

        return _values[_values.Length - 1];
    }
}

public class FootPath
{
    private const int _keyframes = 5;
    private const int _lastKeyframe = _keyframes - 1;

    public float duration => _times[_lastKeyframe];

    private readonly float[] _times = new float[_keyframes];
    private readonly Vector3[] _positions = new Vector3[_keyframes];
    private readonly Quaternion[] _yaws = new Quaternion[_keyframes];
    private readonly float[] _footPitches = new float[_keyframes];
    private readonly float[] _footPitchWeight = new float[_keyframes];
    private readonly float[] _toePitches = new float[_keyframes];

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

    public float EvaluateFootPitch(float t)
    {
        return Evaluate(t, _footPitches, Mathf.Lerp);
    }

    public float EvaluateFootPitchWeight(float t)
    {
        return Evaluate(t, _footPitchWeight, Mathf.Lerp);
    }

    public float EvaluateToePitch(float t)
    {
        return Evaluate(t, _toePitches, Mathf.Lerp);
    }

    public void Set(int i, float t, Vector3 position, Quaternion yaw, float pitch, float pitchWeight, float toePitch)
    {
        _times[i] = t;
        _positions[i] = position;
        _yaws[i] = yaw;
        _footPitches[i] = pitch;
        _footPitchWeight[i] = pitchWeight;
        _toePitches[i] = toePitch;
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
