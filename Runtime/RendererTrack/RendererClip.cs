using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MaterialTrack
{
public class RendererClip : PlayableAsset, ITimelineClipAsset
{
    public RendererBehaviour template = new RendererBehaviour();
    public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        => ScriptPlayable<RendererBehaviour>.Create(graph, template);
}
}
