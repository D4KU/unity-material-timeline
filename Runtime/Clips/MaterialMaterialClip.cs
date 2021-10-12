using UnityEngine.Timeline;

public class MaterialMaterialClip : MaterialClip<MaterialMaterialBehaviour>, ITimelineClipAsset
{
    public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;
}
