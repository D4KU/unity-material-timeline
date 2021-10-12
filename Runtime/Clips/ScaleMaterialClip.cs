using UnityEngine.Timeline;

public class ScaleMaterialClip : MaterialClip<ScaleMaterialBehaviour>, ITimelineClipAsset
{
    public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;
}
