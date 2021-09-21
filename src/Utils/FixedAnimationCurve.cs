using UnityEngine;

public class FixedAnimationCurve
{
    private readonly Keyframe[] _keys;
    private readonly AnimationCurve _curve;

    public float duration => _curve[_curve.length - 1].time;

    public FixedAnimationCurve()
    {
        _keys = new[] { new Keyframe(0, 0), new Keyframe(1f, 0), new Keyframe(2f, 0), new Keyframe(3f, 0), new Keyframe(4f, 0) };
        _curve = new AnimationCurve { preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, keys = _keys };
    }

    public void Sync()
    {
        _curve.keys = _keys;
        for (var i = 1; i < _curve.length - 1; i++)
            _curve.SmoothTangents(i, 1);
    }

    public float Evaluate(float t)
    {
        return _curve.Evaluate(t);
    }

    public float GetValueAtKey(int index)
    {
        return _curve[index].value;
    }

    public void MoveKey(int index, Keyframe keyframe)
    {
        _keys[index] = keyframe;
    }
}
