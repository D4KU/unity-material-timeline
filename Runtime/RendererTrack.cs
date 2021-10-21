using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackBindingType(typeof(Renderer))]
[TrackColor(1, 0, 0)]
[TrackClipType(typeof(MaterialClip))]
public class RendererTrack : TrackAsset, ILayerable
{
    public RendererMixer template = new RendererMixer();

    /// <inheritdoc cref="ILayerable.CreateLayerMixer"/>
    public Playable CreateLayerMixer(
        PlayableGraph graph,
        GameObject go,
        int inputCount)
    {
        return ScriptPlayable<MaterialLayerMixer>.Create(graph, inputCount);
    }

    public override Playable CreateTrackMixer(
        PlayableGraph graph, GameObject go, int inputCount)
    {
        var playable = ScriptPlayable<RendererMixer>.Create(graph, template, inputCount);
        var mixer = playable.GetBehaviour();

        // Set display name of each clip
        foreach (TimelineClip clip in GetClips())
        {
            MaterialBehaviour data = ((MaterialClip)clip.asset).template;
            clip.displayName = $"{data.propertyName} [{data.propertyType}]";

            // Set mixer
            data.mixer = mixer;
        }
        return playable;
    }

#if UNITY_EDITOR
    public override void GatherProperties(
        PlayableDirector director,
        IPropertyCollector driver)
    {
        var trackBinding = director.GetGenericBinding(this);
        if (trackBinding == null)
            return;

        var serializedObject = new UnityEditor.SerializedObject(trackBinding);
        var iterator = serializedObject.GetIterator();

        while (iterator.NextVisible(true))
        {
            if (!iterator.hasVisibleChildren)
                driver.AddFromName(iterator.propertyPath);
        }

        base.GatherProperties(director, driver);
    }
#endif
}

