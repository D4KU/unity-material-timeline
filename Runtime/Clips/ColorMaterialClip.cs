using UnityEngine.Timeline;

public class ColorMaterialClip : MaterialClip<ColorMaterialBehaviour>, ITimelineClipAsset
{
    public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;
}
