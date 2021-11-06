using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using UnityEditor.Timeline;
using System.Collections.Generic;
using U = UnityEngine.Rendering.ShaderPropertyType;

namespace MaterialTrack
{
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
        // Draw dropdown to choose a shader property to manipulate
        DrawPropertyDropdown(position, property);
        SerializedProperty nameProp = property.FindPropertyRelative(T.NAME_FIELD);

        // Draw UI to manipulate the chosen shader property
        if (!string.IsNullOrEmpty(nameProp.stringValue))
        {
            DrawValueProperty(property);
            RefreshObject(property.serializedObject);
        }
    }

    /// <summary>
    /// The label used next to value property fields
    /// </summary>
    protected GUIContent ValueLabel => new GUIContent("Value");

    /// <summary>
    /// Based upon the chosen property name in the dropdown, draw the value
    /// field of the corresponding type.
    /// </summary>
    protected void DrawValueProperty(SerializedProperty root)
    {
        SerializedProperty typeP = root.FindPropertyRelative(T.TYPE_FIELD);
        SerializedProperty vecP  = root.FindPropertyRelative(T.VEC_FIELD);
        Vector4 vecVal = vecP.vector4Value;

        // Depending on the chosen shader property type, decide which
        // field to draw
        switch ((U)typeP.enumValueIndex)
        {
            case U.Texture:
                DrawTextureField(root);
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

    /// <summary>
    /// Draw an error message box if the chosen shader property doesn't match
    /// the given texture dimension
    /// </summary>
    protected void MaybeDrawTextureDimensionErrorBox(
        SerializedProperty root, TextureDimension matchTo)
    {
        // Chosen shader property name
        SerializedProperty nameP = root.FindPropertyRelative(T.NAME_FIELD);

        // Draw box if any material this behaviour manipulates has a texture
        // property of the same name, with a different dimension.
        foreach (Material mat in GetAffectedMaterials(root))
        {
            TextureDimension propertyDimension =
                mat.shader.GetPropertyTextureDimension(nameP.stringValue);

            if (propertyDimension != matchTo)
            {
                string msg = "You can't assign a texture with dimension " +
                    $"{matchTo} to a property with dimension " +
                    $"{propertyDimension}.";
                EditorGUILayout.HelpBox(msg, MessageType.Error);
                return;
            }
        }
    }

    /// <summary>
    /// Draw texture value field with a field for the default color
    /// </summary>
    protected void DrawTextureField(SerializedProperty root)
    {
        SerializedProperty texP = root.FindPropertyRelative(T.TEX_FIELD);
        EditorGUILayout.PropertyField(texP, ValueLabel);

        // The UI would be nicer here if we would render an object field
        // of the Texture subclass the chosen property actually expects.
        // The problem are RenderTextures, which don't inherit from
        // Texture2D, but are indistinguishable from them for shaders.
        // So instead, all we can do is to render a Texture base class field
        // and display an error when the linked texture dimension is
        // incorrect.
        if (texP.objectReferenceValue is Texture texture)
            MaybeDrawTextureDimensionErrorBox(root, texture.dimension);

        SerializedProperty vecP = root.FindPropertyRelative(T.VEC_FIELD);
        GUIContent defLabel = new GUIContent("Default Color", "When this " +
            "texture is blended and no other clip is available to " +
            "supply the second texture, the texture is blended with " +
            "this color instead. It is also used if no texture is set.");
        vecP.vector4Value = EditorGUILayout.ColorField(defLabel, vecP.vector4Value);
    }

    /// <summary>
    /// Returns the materials the currently drawn behaviour manipulates.
    /// Never returns null.
    /// </summary>
    protected IEnumerable<Material> GetAffectedMaterials(SerializedProperty property)
    {
        // Object whose inspector is currently drawn.
        UnityEngine.Object targetObject = property.serializedObject.targetObject;

        // Object this drawer renders. It's a field of 'targetObject'.
        if (fieldInfo.GetValue(targetObject) is IMaterialProvider target
                && target.Materials != null)
            return target.Materials;

        // Ensure that the timeline rebuilds the graph, so that
        // the material provider could initialize
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
        return new Material[0];
    }

    /// <summary>
    /// Draw a searchable dropdown from which to chose the name of the shader
    /// property to manipulate
    /// </summary>
    protected void DrawPropertyDropdown(Rect position, SerializedProperty root)
    {
        // The currently chosen shader property name
        SerializedProperty nameProp = root.FindPropertyRelative(T.NAME_FIELD);

        // Draw dropdown button
        Rect dropdownLabel = EditorGUI.PrefixLabel(
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

        if (dropdownOpen)
            DrawMaterialPropertyList(dropdownLabel, root);
    }

    /// <summary>
    /// Draw a searchable list of shader properties found in the given materials
    /// </summary>
    protected void DrawMaterialPropertyList(Rect position, SerializedProperty root)
    {
        // Materials to list properties of
        IEnumerable<Material> materials = GetAffectedMaterials(root);

        // Collect all unique property names to fill list with
        var propertyNames = new HashSet<string>();
        foreach (Material mat in materials)
        {
            // Can't pass all materials directly to GetMaterialProperties(),
            // because it demands that they share all properties
            var props = MaterialEditor.GetMaterialProperties(
                new Material[] { mat });

            foreach (MaterialProperty p in props)
                propertyNames.Add(p.name);
        }

        // Create callback function when dropdown entry got selected
        Action<string> OnSelectionChanged = entry =>
        {
            // Choose the first material that has the selected property
            // to retrieve the corresponding shader property type
            foreach (Material mat in materials)
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
                    // Pack range limits into unused vector components
                    Vector2 limits = shader.GetPropertyRangeLimits(propIndex);
                    vec.y = limits.x;
                    vec.z = limits.y;
                }

                var nameProp = root.FindPropertyRelative(T.NAME_FIELD);
                var typeProp = root.FindPropertyRelative(T.TYPE_FIELD);
                var vecProp  = root.FindPropertyRelative(T.VEC_FIELD);

                nameProp.stringValue = entry;
                typeProp.enumValueIndex = (int)propType;
                vecProp.vector4Value = vec;

                // Ensure selected entry is triggering updates immediately
                RefreshObject(root.serializedObject);
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
                return;
            }
        };

        // Build dropdown popup and show it
        var treeView = new StringTreeView(propertyNames, OnSelectionChanged);
        var treeViewPopup = new TreeViewPopupWindow(treeView)
        {
            Width = position.width
        };
        PopupWindow.Show(position, treeViewPopup);
    }

    protected void RefreshObject(SerializedObject toRefresh)
    {
        toRefresh.ApplyModifiedProperties();
        toRefresh.Update();
    }
}
}
