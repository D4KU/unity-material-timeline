using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RendererMixer))]
public class RendererMixerDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        => 0f;

    public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        => EditorGUILayout.PropertyField(
            property.FindPropertyRelative(RendererMixer.MAT_IDX_FIELD));
}
