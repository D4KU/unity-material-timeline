using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackBindingType(typeof(Material))]
[TrackColor(.5f, .84f, 1f)]
[TrackClipType(typeof(MaterialClip))]
public class MaterialTrack : TrackAsset
{
    public override Playable CreateTrackMixer(
            PlayableGraph graph, GameObject go, int inputCount)
    {
        // Set display name of each clip
        foreach (TimelineClip clip in GetClips())
        {
            MaterialBehaviour data = ((MaterialClip)clip.asset).data;
            clip.displayName = $"{data.propertyName} [{data.propertyType}]";
        }
        return ScriptPlayable<MaterialMixer>.Create(graph, inputCount);
    }
}
