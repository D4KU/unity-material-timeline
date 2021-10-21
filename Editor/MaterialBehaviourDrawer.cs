using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomPropertyDrawer(typeof(MaterialBehaviour))]
public class MaterialBehaviourDrawer : PropertyDrawer
{
    float LineHeight
        => EditorGUIUtility.singleLineHeight
         + EditorGUIUtility.standardVerticalSpacing;

    public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        => EditorGUIUtility.singleLineHeight;

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

        var prefixLabel = EditorGUI.PrefixLabel(
            singleFieldRect,
            new GUIContent("Property"));

        object targetObject = property.serializedObject.targetObject;
        var target = (MaterialBehaviour)fieldInfo.GetValue(targetObject);

        if (EditorGUI.DropdownButton(
            prefixLabel,
            new GUIContent(target.propertyName),
            FocusType.Passive))
        {
            Material[] affectedMaterials = target.mixer.AffectedMaterials;
            var props = MaterialEditor.GetMaterialProperties(affectedMaterials);

            Action<string> OnSelectionChanged = entry =>
            {
                target.propertyName = entry;
                foreach (Material mat in affectedMaterials)
                {
                    Shader shader = mat.shader;
                    int propIndex = shader.FindPropertyIndex(entry);
                    if (propIndex < 0)
                        // Shader doesn't have any property with selected name
                        continue;

                    target.propertyType = shader.GetPropertyType(propIndex);
                    break;
                }
            };

            var treeView = new StringTreeView(
                props.Select(i => i.name),
                OnSelectionChanged);

            var treeViewPopup = new TreeViewPopupWindow(treeView)
            {
                Width = prefixLabel.width
            };
            PopupWindow.Show(prefixLabel, treeViewPopup);
        }

        var valueProp = property.FindPropertyRelative(target.FieldName);
        if (valueProp != null)
            EditorGUILayout.PropertyField(valueProp, new GUIContent("Value"));
    }
}
