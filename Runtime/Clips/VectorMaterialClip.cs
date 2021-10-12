using UnityEngine.Timeline;

public class VectorMaterialClip : MaterialClip<VectorMaterialBehaviour>, ITimelineClipAsset
{
    public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;
}
