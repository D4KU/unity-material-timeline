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
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<MaterialMixer>.Create(graph, inputCount);
            PlayableDirector director = go.GetComponent<PlayableDirector>();
            if (director == null)
                return mixer;

            MaterialMixer behaviour = mixer.GetBehaviour();
            behaviour.clips = GetClips();
            behaviour.director = director;

            return mixer;
        }
    }
}
