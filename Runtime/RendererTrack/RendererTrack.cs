using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MaterialTrack
{
[TrackBindingType(typeof(Renderer))]
[TrackColor(.1f, .3f, .7f)]
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
            clip.displayName = BuildClipName(data);

            // The track mixer created in this class is the object providing
            // each clip's behaviour access to the materials of the bound
            // renderer.
            data.provider = provider;
        }
    }

    /// <summary>
    /// Build string shown on clips from a clip's data
    /// </summary>
    public static string BuildClipName(RendererBehaviour data)
    {
        if (string.IsNullOrWhiteSpace(data.propertyName))
            return "Empty";
        else
            return $"{data.propertyName} [{data.propertyType}]";

    }
}
}
