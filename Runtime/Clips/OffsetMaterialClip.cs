using UnityEngine.Timeline;

public class OffsetMaterialClip : MaterialClip<OffsetMaterialBehaviour>, ITimelineClipAsset
{
    public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;
}
