using UnityEditor;
using UnityEngine;
using System;

namespace CustomTimeline
{
    using PropertyType = MaterialBehaviour.PropertyType;

    [CustomPropertyDrawer(typeof(MaterialBehaviour))]
    public class MaterialBehaviourDrawer : PropertyDrawer
    {
        float LineHeight
            => EditorGUIUtility.singleLineHeight
             + EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(
                SerializedProperty property,
                GUIContent label)
            => 3 * LineHeight;

        public override void OnGUI(
                Rect position,
                SerializedProperty property,
                GUIContent label)
        {
            Rect singleFieldRect = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);

            var typeProp = property.FindPropertyRelative("propertyType");
            EditorGUI.PropertyField(singleFieldRect, typeProp);
            singleFieldRect.y += LineHeight;

            var propType = (PropertyType)typeProp.enumValueIndex;
            if (propType != PropertyType.Material)
            {
                var nameProp = property.FindPropertyRelative("propertyName");
                EditorGUI.PropertyField(singleFieldRect, nameProp);
                singleFieldRect.y += LineHeight;
            }

            string valuePropName = propType switch
            {
                PropertyType.Int => "intValue",
                PropertyType.Float => "floatValue",
                PropertyType.Color => "color",
                PropertyType.Texture => "texture",
                PropertyType.TextureTiling => "tiling",
                PropertyType.TextureOffset => "offset",
                PropertyType.Vector => "vector",
                PropertyType.Material => "material",
                _ => throw new ArgumentOutOfRangeException(),
            };

            var valueProp = property.FindPropertyRelative(valuePropName);
            EditorGUI.PropertyField(singleFieldRect, valueProp);
        }
    }
}
