using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MaterialTrack
{
public class SlotClip : PlayableAsset, ITimelineClipAsset
{
    public SlotBehaviour template;
    public ClipCaps clipCaps => ClipCaps.Extrapolation;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        => ScriptPlayable<SlotBehaviour>.Create(graph, template);
}
}
