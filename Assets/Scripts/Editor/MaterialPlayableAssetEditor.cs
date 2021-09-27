using UnityEngine;
using UnityEditor;

namespace CustomTimeline
{
    [CustomEditor(typeof(MaterialPlayableAsset))]
    internal sealed class MaterialPlayableAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var playable = target as MaterialPlayableAsset;

            playable.property.propertyName = EditorGUILayout.TextField("プロパティ名：", playable.property.propertyName);

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
