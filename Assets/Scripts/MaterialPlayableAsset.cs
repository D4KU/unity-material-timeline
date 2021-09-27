using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTimeline
{
    public enum PropertyType
    {
        Int,
        Float,
        Texture,
        TextureTiling,
        TextureOffset,
        Color,
        Vector
    }

    //TODO:使用する値のみを保持したい
    //使用しない値も確保している
    //型毎にクリップを作成する？
    [System.Serializable]
    public class MaterialProperty
    {
        public string propertyName;
        public PropertyType propertyType;
        public int intValue;
        public float floatValue;
        public Texture texture;
        public Vector2 tilling;
        public Vector2 offset;
        public Color color;
        public Vector4 vector;
    }

    [System.Serializable]
    public class MaterialPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        [HideInInspector]
        public MaterialProperty property = new MaterialProperty();

        public ClipCaps clipCaps
        {
            get
            {
                if (property.propertyType != PropertyType.Texture)
                    return ClipCaps.Blending | ClipCaps.SpeedMultiplier;
                else
                    return ClipCaps.None;
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            MaterialBehaviour behaviour = new MaterialBehaviour();
            return ScriptPlayable<MaterialBehaviour>.Create(graph, behaviour);
        }

    }
}
