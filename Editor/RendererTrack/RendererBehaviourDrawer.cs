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

        var nameProp = property.FindPropertyRelative(T.NAME_FIELD);
        if (!string.IsNullOrEmpty(nameProp.stringValue))
        {
            DrawValueProperty(property);
            Refresh(property.serializedObject);
        }
    }

    protected void DrawTextureField(SerializedProperty root)
    {
        var texP = root.FindPropertyRelative(T.TEX_FIELD);
        var vecP = root.FindPropertyRelative(T.VEC_FIELD);
        EditorGUILayout.PropertyField(texP, ValueLabel);

        var defLabel = new GUIContent("Default Color", "When this " +
            "texture is blended and no other clip is available to " +
            "supply the second texture, the texture is blended with " +
            "this color instead. It is also used if no texture is set.");
        vecP.vector4Value = EditorGUILayout.ColorField(defLabel, vecP.vector4Value);
    }

    protected GUIContent ValueLabel => new GUIContent("Value");

    protected void DrawValueProperty(SerializedProperty property)
    {
        var typeP = property.FindPropertyRelative(T.TYPE_FIELD);
        var vecP  = property.FindPropertyRelative(T.VEC_FIELD);
        var vecVal = vecP.vector4Value;

        switch ((U)typeP.enumValueIndex)
        {
            case U.Texture:
                DrawTextureField(property);
                break;
            case U.Color:
                vecP.vector4Value = EditorGUILayout.ColorField(ValueLabel, vecVal);
                break;
            case U.Vector:
                EditorGUILayout.PropertyField(vecP, ValueLabel);
                break;
            case U.Float:
                vecVal.x = EditorGUILayout.FloatField(ValueLabel, vecVal.x);
                vecP.vector4Value = vecVal;
                break;
            case U.Range:
                vecVal.x = EditorGUILayout.Slider(
                    ValueLabel,
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

        var nameProp = property.FindPropertyRelative(T.NAME_FIELD);
        var dropdownLabel = EditorGUI.PrefixLabel(
            new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight),
            new GUIContent("Property"));
        bool dropdownOpen = EditorGUI.DropdownButton(
            dropdownLabel,
            new GUIContent(nameProp.stringValue),
            FocusType.Keyboard);

        if (affectedMaterials == null)
        {
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
            return;
        }

        if (dropdownOpen)
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

