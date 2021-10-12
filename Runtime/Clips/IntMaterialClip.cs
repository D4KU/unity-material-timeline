using UnityEngine.Timeline;

public class IntMaterialClip : MaterialClip<IntMaterialBehaviour>, ITimelineClipAsset
{
    public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;
}
