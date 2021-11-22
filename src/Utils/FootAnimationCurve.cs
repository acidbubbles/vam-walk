using System;
using UnityEngine;

public class FootAnimationCurve<T>
{
    public float duration => _times[_times.Length - 1];

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
        t = Mathf.Clamp(t, 0f, duration);

        for (var i = 1; i < _values.Length; i++)
        {
            if (t < _times[i])
                return _interpolate(_values[i - 1], _values[i], (t - _times[i - 1]) / (_times[i] - _times[i - 1]));
        }

        return _values[_values.Length - 1];
    }
}
