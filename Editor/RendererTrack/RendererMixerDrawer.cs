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
        => 0;

    public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
    {
        var rendererProp = property.FindPropertyRelative(
            nameof(RendererMixer.boundRenderer));
        if (!(rendererProp.objectReferenceValue is Renderer renderer))
            return;

        int slotCount = renderer.sharedMaterials.Length;
        var maskProp = property.FindPropertyRelative(nameof(RendererMixer.mask));
        int oldMaskSize = maskProp.arraySize;

        // RendererTrack resizes the mask if this drawer isn't shown,
        // and this drawer resizes the mask when the timeline isn't playing
        if (oldMaskSize != slotCount)
        {
            maskProp.arraySize = slotCount;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorGUILayout.LabelField("Slots to affect");

        // For each available material slot, draw a toggle left from a
        // read-only field with the initially assigned material.
        for (int i = 0; i < slotCount; i++)
        {
            SerializedProperty maskElemProp = maskProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();

            // Draw toggle
            // Set value to true if toggle just got created
            maskElemProp.boolValue = EditorGUILayout.Toggle(
                value: i >= oldMaskSize | maskElemProp.boolValue,
                options: GUILayout.Width(20));

            // Draw material field
            bool guiFormerlyEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.ObjectField(
                obj: renderer.sharedMaterials[i],
                objType: typeof(Material),
                allowSceneObjects: false);
            GUI.enabled = guiFormerlyEnabled;

            EditorGUILayout.EndHorizontal();
        }

        property.serializedObject.ApplyModifiedProperties();
    }
}
}
