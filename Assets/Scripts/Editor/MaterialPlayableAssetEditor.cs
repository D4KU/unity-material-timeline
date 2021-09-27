using UnityEngine;
using UnityEditor;

namespace CustomTimeline
{
    [CustomEditor(typeof(MaterialPlayableAsset))]
    internal sealed class MaterialPlayableAssetEditor : Editor
    {
        const string VALUE_LABEL = "Property value";
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            MaterialProperty prop = (target as MaterialPlayableAsset).property;
            prop.propertyName = EditorGUILayout.TextField("Property name", prop.propertyName);
            prop.propertyType = (PropertyType)EditorGUILayout.EnumPopup("Property type", prop.propertyType);

            switch (prop.propertyType)
            {
                case PropertyType.Int:
                    prop.intValue = EditorGUILayout.IntField(VALUE_LABEL, prop.intValue);
                    break;
                case PropertyType.Float:
                    prop.floatValue = EditorGUILayout.FloatField(VALUE_LABEL, prop.floatValue);
                    break;
                case PropertyType.Color:
                    prop.color = EditorGUILayout.ColorField(VALUE_LABEL, prop.color);
                    break;
                case PropertyType.Texture:
                    prop.texture = (Texture)EditorGUILayout.ObjectField(VALUE_LABEL, prop.texture, typeof(Texture), false);
                    break;
                case PropertyType.TextureTiling:
                    prop.tilling = EditorGUILayout.Vector2Field(VALUE_LABEL, prop.tilling);
                    break;
                case PropertyType.TextureOffset:
                    prop.offset = EditorGUILayout.Vector2Field(VALUE_LABEL, prop.offset);
                    break;
                case PropertyType.Vector:
                    prop.vector = EditorGUILayout.Vector4Field(VALUE_LABEL, prop.vector);
                    break;
                default:
                    break;
            }
        }
    }
}
