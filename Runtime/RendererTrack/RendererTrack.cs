using System;
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
    public const string EMPTY_SLOT_NAME = "Empty";
    public RendererMixer template;

    /// <inheritdoc cref="ILayerable.CreateLayerMixer"/>
    public Playable CreateLayerMixer(
        PlayableGraph graph,
        GameObject go,
        int inputCount)
        => ScriptPlayable<MaterialLayerMixer>.Create(graph, inputCount);

    public override Playable CreateTrackMixer(
        PlayableGraph graph, GameObject go, int inputCount)
    {
        InitializeTemplate(go);
        var mixer = ScriptPlayable<RendererMixer>.Create(graph, template, inputCount);
        InitializeClips(mixer.GetBehaviour());
        return mixer;
    }

    void InitializeTemplate(GameObject go)
    {
        if (!this.TryGetBinding(go, out Renderer renderer))
            return;

        template.boundRenderer = renderer;
        ExtensionMethods.ResizeArray(
            array: ref template.mask,
            newSize: renderer.sharedMaterials.Length,
            defaultValue: true);
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
            return EMPTY_SLOT_NAME;
        return $"{data.propertyName} [{data.propertyType}]";
    }
}
}
