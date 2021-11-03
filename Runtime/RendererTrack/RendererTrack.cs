using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MaterialTrack
{
[TrackBindingType(typeof(Renderer))]
[TrackColor(1, 0, 0)]
[TrackClipType(typeof(RendererClip))]
public class RendererTrack : TrackAsset, ILayerable
{
    public RendererMixer template = new RendererMixer();

    /// <inheritdoc cref="ILayerable.CreateLayerMixer"/>
    public Playable CreateLayerMixer(
        PlayableGraph graph,
        GameObject go,
        int inputCount)
        => ScriptPlayable<MaterialLayerMixer>.Create(graph, inputCount);

    public override Playable CreateTrackMixer(
        PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixer = ScriptPlayable<RendererMixer>.Create(graph, template, inputCount);
        InitializeClips(mixer.GetBehaviour());
        return mixer;
    }

    void InitializeClips(IMaterialProvider provider)
    {
        foreach (TimelineClip clip in GetClips())
        {
            // Set display name of each clip
            var data = ((RendererClip)clip.asset).template;
            clip.displayName = $"{data.propertyName} [{data.propertyType}]";

            // The track mixer created in this class is the object providing
            // each clip's behaviour access to the materials of the bound
            // renderer.
            data.provider = provider;
        }
    }
}
}
