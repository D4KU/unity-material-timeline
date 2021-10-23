using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackBindingType(typeof(Renderer))]
[TrackColor(1, 0, 0)]
[TrackClipType(typeof(RendererClip))]
public class RendererTrack : MaterialTrack
{
    public RendererMixer template = new RendererMixer();

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

            // Set material provider
            data.provider = provider;
        }
    }
}
