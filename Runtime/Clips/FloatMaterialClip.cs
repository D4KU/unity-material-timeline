using UnityEngine.Timeline;

public class FloatMaterialClip : MaterialClip<FloatMaterialBehaviour>, ITimelineClipAsset
{
    public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;
}
