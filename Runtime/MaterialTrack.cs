using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackBindingType(typeof(Material))]
[TrackColor(.5f, .84f, 1f)]
[TrackClipType(typeof(MaterialClip))]
public class MaterialTrack : TrackAsset, ILayerable
{
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
        // Set display name of each clip
        foreach (TimelineClip clip in GetClips())
        {
            MaterialBehaviour data = ((MaterialClip)clip.asset).template;
            clip.displayName = $"{data.propertyName} [{data.propertyType}]";
        }
        return ScriptPlayable<MaterialMixer>.Create(graph, inputCount);
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
