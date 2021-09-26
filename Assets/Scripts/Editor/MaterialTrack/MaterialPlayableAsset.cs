using UnityEngine;
using UnityEditor;
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

    [CustomEditor(typeof(MaterialPlayableAsset))]
    internal sealed class MaterialPlayableAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var playable = target as MaterialPlayableAsset;

            playable.property.propertyName = EditorGUILayout.TextField("プロパティ名：",playable.property.propertyName);

            playable.property.propertyType = (PropertyType)EditorGUILayout.EnumPopup(playable.property.propertyType);

            switch (playable.property.propertyType)
            {
                case PropertyType.Int:
                    playable.property.intValue = EditorGUILayout.IntField("value : ", playable.property.intValue);
                    break;
                case PropertyType.Float:
                    playable.property.floatValue = EditorGUILayout.FloatField("value : ", playable.property.floatValue);
                    break;
                case PropertyType.Color:
                    playable.property.color = EditorGUILayout.ColorField("value : ", playable.property.color);
                    break;
                case PropertyType.Texture:
                    playable.property.texture = (Texture)EditorGUILayout.ObjectField("value : ", playable.property.texture, typeof(Texture), false);
                    break;
                case PropertyType.TextureTiling:
                    playable.property.tilling = EditorGUILayout.Vector2Field("value : ", playable.property.tilling);
                    break;
                case PropertyType.TextureOffset:
                    playable.property.offset = EditorGUILayout.Vector2Field("value : ", playable.property.offset);
                    break;
                case PropertyType.Vector:
                    playable.property.vector = EditorGUILayout.Vector4Field("value : ", playable.property.vector);
                    break;
                default:
                    break;
            }
        }
    }
}
