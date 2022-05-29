using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace MaterialTrack
{
[TrackBindingType(typeof(Renderer))]
[TrackColor(0.4f, .4f, .7f)]
[TrackClipType(typeof(SlotClip))]
public class SlotTrack : TrackAsset, ILayerable
{
    public SlotMixer template;

    /// <inheritdoc cref="ILayerable.CreateLayerMixer"/>
    public Playable CreateLayerMixer(
            PlayableGraph graph,
            GameObject go,
            int inputCount)
        => ScriptPlayable<SlotLayerMixer>.Create(graph, inputCount);

    public override Playable CreateTrackMixer(
            PlayableGraph graph, GameObject go, int inputCount)
    {
        InitializeClips();
        InitializeTemplate(go);
        return ScriptPlayable<SlotMixer>.Create(graph, template, inputCount);
    }

    void InitializeTemplate(GameObject go)
    {
        if (!(go.GetComponent<PlayableDirector>()
                .GetGenericBinding(isSubTrack ? parent : this)
                is Renderer renderer))
            return;

        template.boundRenderer = renderer;
        template.initialMaterials = renderer.sharedMaterials.ToArray();
        int materialCount = template.initialMaterials.Length;

        if (materialCount != template.mask.Length)
        {
            Array.Resize(ref template.mask, materialCount);
            for (int i = template.mask.Length - 1; i < materialCount; i++)
                template.mask[i] = true;
        }
    }

    void InitializeClips()
    {
        foreach (TimelineClip clip in GetClips())
        {
            // Set display name of each clip
            var data = ((SlotClip)clip.asset).template;
            clip.displayName = BuildClipName(data);
        }
    }

    /// <summary>
    /// Build string shown on clips from a clip's data
    /// </summary>
    public static string BuildClipName(SlotBehaviour data)
    {
        if (data.material == null)
            return RendererTrack.EMPTY_SLOT_NAME;
        return data.material.name;
    }
}
}
