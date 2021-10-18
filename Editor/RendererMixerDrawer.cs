using UnityEditor;
using UnityEngine;
using System;

[CustomPropertyDrawer(typeof(RendererMixer))]
public class RendererMixerDrawer : PropertyDrawer
{
    float LineHeight
        => EditorGUIUtility.singleLineHeight
         + EditorGUIUtility.standardVerticalSpacing;

    public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        => LineHeight;

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

        var typeProp = property.FindPropertyRelative("materialIndex");
        EditorGUI.PropertyField(singleFieldRect, typeProp);
    }
}
