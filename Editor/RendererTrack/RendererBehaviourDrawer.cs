using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor.Timeline;
using System.Collections.Generic;
using U = UnityEngine.Rendering.ShaderPropertyType;
using T = RendererBehaviour;

[CustomPropertyDrawer(typeof(T))]
public class RendererBehaviourDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        => EditorGUIUtility.singleLineHeight;

    public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
    {
        DrawPropertyDropdown(position, property);
        DrawValueProperty(property);
        Refresh(property.serializedObject);
    }

    protected void DrawValueProperty(SerializedProperty property)
    {
        var typeP = property.FindPropertyRelative(T.TYPE_FIELD);
        var vecP  = property.FindPropertyRelative(T.VEC_FIELD);
        var valLabel = new GUIContent("Value");
        var vecVal = vecP.vector4Value;

        switch ((U)typeP.enumValueIndex)
        {
            case U.Texture:
                var texProp = property.FindPropertyRelative(T.TEX_FIELD);
                EditorGUILayout.PropertyField(texProp, valLabel);
                break;
            case U.Color:
                vecP.vector4Value = EditorGUILayout.ColorField(valLabel, vecVal);
                break;
            case U.Vector:
                EditorGUILayout.PropertyField(vecP, valLabel);
                break;
            case U.Float:
                vecVal.x = EditorGUILayout.FloatField(valLabel, vecVal.x);
                vecP.vector4Value = vecVal;
                break;
            case U.Range:
                vecVal.x = EditorGUILayout.Slider(
                    valLabel,
                    vecVal.x,
                    vecVal.y,
                    vecVal.z);
                vecP.vector4Value = vecVal;
                break;
        }
    }

    protected void DrawPropertyDropdown(Rect position, SerializedProperty property)
    {
        var targetObject = property.serializedObject.targetObject;
        var target = fieldInfo.GetValue(targetObject) as IMaterialProvider;
        var affectedMaterials = target.Materials;
        if (affectedMaterials == null)
            return;

        var nameProp = property.FindPropertyRelative(T.NAME_FIELD);
        var dropdownLabel = EditorGUI.PrefixLabel(
            new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight),
            new GUIContent("Property"));

        if (EditorGUI.DropdownButton(
            dropdownLabel,
            new GUIContent(nameProp.stringValue),
            FocusType.Keyboard))
        {
            // Create callback function when dropdown entry got selected
            Action<string> OnSelectionChanged = entry =>
            {
                foreach (Material mat in affectedMaterials)
                {
                    Shader shader = mat.shader;
                    int propIndex = shader.FindPropertyIndex(entry);
                    if (propIndex < 0)
                        // Shader doesn't have any property with selected name
                        continue;

                    Vector4 vec = Vector4.one;
                    U propType = shader.GetPropertyType(propIndex);
                    if (propType == U.Range)
                    {
                        Vector2 limits = shader.GetPropertyRangeLimits(propIndex);
                        vec.y = limits.x;
                        vec.z = limits.y;
                    }

                    var nameProp = property.FindPropertyRelative(T.NAME_FIELD);
                    var typeProp = property.FindPropertyRelative(T.TYPE_FIELD);
                    var vecProp  = property.FindPropertyRelative(T.VEC_FIELD);

                    nameProp.stringValue = entry;
                    typeProp.enumValueIndex = (int)propType;
                    vecProp.vector4Value = vec;

                    Refresh(property.serializedObject);
                    TimelineEditor.Refresh(RefreshReason.ContentsModified);
                    break;
                }
            };

            // Draw dropdown content
            var propertyNames = new List<string>();
            foreach (Material mat in affectedMaterials)
            {
                var props = MaterialEditor.GetMaterialProperties(new Material[]{mat});
                propertyNames.AddRange(props.Select(i => i.name));
            }

            var treeView = new StringTreeView(propertyNames, OnSelectionChanged);
            var treeViewPopup = new TreeViewPopupWindow(treeView)
            {
                Width = dropdownLabel.width
            };
            PopupWindow.Show(dropdownLabel, treeViewPopup);
        }
    }

    protected void Refresh(SerializedObject toRefresh)
    {
        toRefresh.ApplyModifiedProperties();
        toRefresh.Update();
    }
}

