using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MaterialBehaviourBase), useForChildren: true)]
public class MaterialBehaviourDrawer : PropertyDrawer
{
    float LineHeight
        => EditorGUIUtility.singleLineHeight
         + EditorGUIUtility.standardVerticalSpacing;

    public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        => 7 * LineHeight;

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

        var nameProp = property.FindPropertyRelative("propertyName");
        EditorGUI.PropertyField(singleFieldRect, nameProp);
        singleFieldRect.y += LineHeight;

        var valueProp = property.FindPropertyRelative("value");
        EditorGUI.PropertyField(singleFieldRect, valueProp, true);
    }
}
