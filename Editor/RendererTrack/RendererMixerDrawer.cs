using UnityEditor;
using UnityEngine;

namespace MaterialTrack
{
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
    {
        // Draw a field to specify which of the renderer's materials to
        // influence
        EditorGUILayout.PropertyField(
                property.FindPropertyRelative(RendererMixer.MAT_IDX_FIELD));
    }
}
}
