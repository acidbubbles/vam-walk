using UnityEngine;

public class FootPath
{
    private const int _keyframes = 5;
    private const int _lastKeyframe = _keyframes - 1;

    public float duration => _times[_lastKeyframe];

    private readonly float[] _times = new float[_keyframes];
    private readonly Vector3[] _positions = new Vector3[_keyframes];
    private readonly Quaternion[] _rotations = new Quaternion[_keyframes];

    public Vector3 GetPositionAtIndex(int i)
    {
        return _positions[i];
    }

    public Vector3 EvaluatePosition(float t)
    {
        // TODO: Use a better curve instead of this
        for (var i = 1; i < _keyframes; i++)
        {
            if (t < _times[i])
                return Vector3.Lerp(_positions[i - 1], _positions[i], (t - _times[i - 1]) / (_times[i] - _times[i - 1]));
        }

        return GetPositionAtIndex(_lastKeyframe);
    }

    public Quaternion GetRotationAtIndex(int i)
    {
        return _rotations[i];
    }

    public Quaternion EvaluateRotation(float t)
    {
        for (var i = 1; i < _keyframes; i++)
        {
            if (t < _times[i])
                return Quaternion.Slerp(_rotations[i - 1], _rotations[i], (t - _times[i - 1]) / (_times[i] - _times[i - 1]));
        }

        return GetRotationAtIndex(_lastKeyframe);
    }

    public void Set(int i, float t, Vector3 position, Quaternion rotation)
    {
        _times[i] = t;
        _positions[i] = position;
        _rotations[i] = rotation;
    }
}
