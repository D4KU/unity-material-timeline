using UnityEngine.Timeline;

public class TextureMaterialClip : MaterialClip<TextureMaterialBehaviour>, ITimelineClipAsset
{
    public ClipCaps clipCaps => ClipCaps.None;
}
