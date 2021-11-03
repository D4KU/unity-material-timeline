using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MaterialTrack
{
public class MaterialClip : PlayableAsset, ITimelineClipAsset
{
    public MaterialBehaviour template = new MaterialBehaviour();
    public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        => ScriptPlayable<MaterialBehaviour>.Create(graph, template);
}
}
