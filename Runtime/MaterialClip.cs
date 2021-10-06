using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class MaterialClip : PlayableAsset, ITimelineClipAsset
{
    public MaterialBehaviour data = new MaterialBehaviour();

    public ClipCaps clipCaps
    {
        get
        {
            if (data.propertyType == MaterialBehaviour.PropertyType.Texture)
                return ClipCaps.Extrapolation;
            else
                return ClipCaps.Extrapolation | ClipCaps.Blending;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        => ScriptPlayable<MaterialBehaviour>.Create(graph, data);
}
