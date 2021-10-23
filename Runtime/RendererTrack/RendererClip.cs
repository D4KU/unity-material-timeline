using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class RendererClip : PlayableAsset, ITimelineClipAsset
{
    public RendererBehaviour template = new RendererBehaviour();

    public ClipCaps clipCaps
    {
        get
        {
            if (template.propertyType == UnityEngine.Rendering.ShaderPropertyType.Texture)
                return ClipCaps.Extrapolation;
            else
                return ClipCaps.Extrapolation | ClipCaps.Blending;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        => ScriptPlayable<RendererBehaviour>.Create(graph, template);
}
