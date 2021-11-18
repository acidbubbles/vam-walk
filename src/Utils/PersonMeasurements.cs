using System.Linq;
using UnityEngine;

public class PersonMeasurements
{
    private readonly DAZBone[] _bones;
    private readonly WalkConfiguration _config;
    private readonly DAZBone _hipBone;

    public float floorToHip { get; private set; }
    public float hipToHead { get; private set; }
    public float floorToHead { get; private set; }

    public PersonMeasurements(DAZBone[] bones, WalkConfiguration style)
    {
        _bones = bones;
        _config = style;
        _hipBone = _bones.First(b => b.name == "hip");
    }

    public void Sync()
    {
        const float feetToHipHeightRatio = 0.992f;
        const float hipToHeadHeightRatio = 0.926f;
        floorToHip = ((MeasureToHip("lFoot") + MeasureToHip("rFoot")) / 2f) * feetToHipHeightRatio + _config.footFloorDistance.val;
        var upper = MeasureToHip("head") * hipToHeadHeightRatio;
        floorToHead = floorToHip + upper;
        hipToHead = floorToHead - floorToHip;
    }

    private float MeasureToHip(string boneName)
    {
        var from = _bones.First(b => b.name == boneName);
        return Measure(from, _hipBone);
    }

    // ReSharper disable once SuggestBaseTypeForParameter
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
