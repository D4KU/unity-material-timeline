using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTimeline
{
    [TrackBindingType(typeof(Material))]
    [TrackColor(1, 0, 0)]
    [TrackClipType(typeof(MaterialPlayableAsset))]
    public class MaterialTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(
                PlayableGraph graph, GameObject go, int inputCount)
            => ScriptPlayable<MaterialMixer>.Create(graph, inputCount);
    }
}
