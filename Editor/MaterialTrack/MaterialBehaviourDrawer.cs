using UnityEditor;
using UnityEngine;

namespace MaterialTrack
{
using T = MaterialBehaviour;

[CustomPropertyDrawer(typeof(T))]
public class MaterialBehaviourDrawer : RendererBehaviourDrawer
{
    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label)
    {
        // Material mode needs less space
        SerializedProperty useMatP = property.FindPropertyRelative(T.USE_MAT_FIELD);
        return useMatP.boolValue ? 0f : base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        // Find out if we only lerp whole materials ("material mode")
        SerializedProperty useMatP = property.FindPropertyRelative(T.USE_MAT_FIELD);
        if (useMatP.boolValue)
        {
            // Material mode doesn't need all the stuff from base class.
            // Just draw a material field.
            SerializedProperty matP = property.FindPropertyRelative(T.MAT_FIELD);
            EditorGUILayout.PropertyField(matP);
        }
        else
        {
            base.OnGUI(position, property, label);
        }

        // Draw material mode toggle
        EditorGUILayout.PropertyField(useMatP);
    }
}
}
