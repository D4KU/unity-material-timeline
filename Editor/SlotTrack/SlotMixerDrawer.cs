using UnityEditor;
using UnityEngine;

namespace MaterialTrack
{
[CustomPropertyDrawer(typeof(SlotMixer))]
public class SlotMixerDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        => 0;

    public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
    {
        var maskProp = property.FindPropertyRelative(nameof(SlotMixer.mask));
        var initProp = property.FindPropertyRelative(nameof(SlotMixer.initialMaterials));

        EditorGUILayout.LabelField("Slots to override");

        // For each available material slot, draw a toggle left from a
        // read-only field with the initially assigned material.
        for (int i = 0; i < initProp.arraySize; i++)
        {
            SerializedProperty maskElemProp = maskProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();

            // Draw toggle
            maskElemProp.boolValue = EditorGUILayout.Toggle(
                value: maskElemProp.boolValue,
                options: GUILayout.Width(20));

            // Draw material field
            bool guiFormerlyEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(
                property: initProp.GetArrayElementAtIndex(i),
                label: GUIContent.none);
            GUI.enabled = guiFormerlyEnabled;

            EditorGUILayout.EndHorizontal();
        }

        property.serializedObject.ApplyModifiedProperties();
    }
}
}
