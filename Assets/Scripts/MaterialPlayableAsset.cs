using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTimeline
{
    [System.Serializable]
    public class MaterialPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public MaterialBehaviour data;

        public ClipCaps clipCaps
        {
            get
            {
                if (data.propertyType == MaterialBehaviour.PropertyType.Texture)
                    return ClipCaps.None;
                else
                    return ClipCaps.Blending | ClipCaps.SpeedMultiplier;
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
            => ScriptPlayable<MaterialBehaviour>.Create(graph, data);
    }
}
