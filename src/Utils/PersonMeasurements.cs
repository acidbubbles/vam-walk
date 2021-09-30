using System.Linq;
using UnityEngine;

public class PersonMeasurements
{
    private const float _skeletonSumToStandHeightRatio = 0.945f;

    private readonly DAZBone[] _bones;
    private readonly GaitStyle _style;
    private readonly DAZBone _hipBone;

    public float hip { get; private set; }
    public float head { get; private set; }

    public PersonMeasurements(DAZBone[] bones, GaitStyle style)
    {
        _bones = bones;
        _style = style;
        _hipBone = _bones.First(b => b.name == "hip");
    }

    public void Sync()
    {
        // TODO: Preview the exact hip ratio line
        // TODO: Fine tune the actual multipliers
        hip = ((MeasureToHip("lFoot") + MeasureToHip("rFoot")) / 2f) * 0.955f + _style.footFloorDistance.val;
        var upper = MeasureToHip("head") * 0.971f;
        head = hip + upper;
    }

    private float MeasureToHip(string boneName)
    {
        var from = _bones.First(b => b.name == boneName);
        return Measure(from, _hipBone);
    }

    private static float Measure(DAZBone from, DAZBone to)
    {
        var bone = from;
        var length = 0f;
        while (true)
        {
            if (bone.parentBone == to || ReferenceEquals(bone.parentBone, null))
                break;

            length += Vector3.Distance(bone.parentBone.transform.position, bone.transform.position);
            bone = bone.parentBone;
        }
        return length;
    }
}
