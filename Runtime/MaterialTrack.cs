using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackBindingType(typeof(Material))]
[TrackColor(1, 0, 0)]
[TrackClipType(typeof(MaterialClipBase))]
public class MaterialTrack : TrackAsset, ILayerable
{
    /// <inheritdoc cref="ILayerable.CreateLayerMixer"/>
    public Playable CreateLayerMixer(
        PlayableGraph graph,
        GameObject go,
        int inputCount)
    {
        return Playable.Null;
    }

    public override Playable CreateTrackMixer(
            PlayableGraph graph, GameObject go, int inputCount)
        => ScriptPlayable<MaterialMixer>.Create(graph, inputCount);
}
