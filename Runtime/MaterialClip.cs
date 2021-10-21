using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class MaterialClip : PlayableAsset, ITimelineClipAsset
{
    public MaterialBehaviour template = new MaterialBehaviour();

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
        => ScriptPlayable<MaterialBehaviour>.Create(graph, template);
}
