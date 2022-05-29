using UnityEditor;
using UnityEngine;

namespace MaterialTrack
{
[CustomPropertyDrawer(typeof(SlotBehaviour))]
public class SlotBehaviourDrawer : PropertyDrawer
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
        EditorGUILayout.PropertyField(property.FindPropertyRelative(
            nameof(SlotBehaviour.material)));
    }
}
}
