using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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
            // Draw property dropdown as in base class
            DrawPropertyDropdown(position, property);

            // Insert more texture options when the chosen property is a
            // texture
            SerializedProperty typeP = property.FindPropertyRelative(T.TYPE_FIELD);
            if (typeP.enumValueIndex == (int)ShaderPropertyType.Texture)
            {
                // Draw option to manipulate texture asset reference,
                // tiling, or offset
                SerializedProperty texTrgP = property.FindPropertyRelative(T.TEX_TARGET_FIELD);
                EditorGUILayout.PropertyField(texTrgP);

                if (texTrgP.enumValueIndex == (int)T.TextureTarget.Asset)
                {
                    // If asset reference is chosen, keep the base class
                    // behavior of drawing a texture reference field
                    DrawTextureField(property);
                }
                else
                {
                    // Otherwise, draw a Vector2 for Tiling or Offset
                    SerializedProperty vecP = property.FindPropertyRelative(T.VEC_FIELD);

                    // Ensure to not overwrite z and w components of 'vecP'
                    Vector2 vec2 =
                        EditorGUILayout.Vector2Field(ValueLabel, vecP.vector4Value);
                    Vector4 vec4 = vecP.vector4Value;
                    vecP.vector4Value = new Vector4(vec2.x, vec2.y, vec4.z, vec4.w);
                }
            }
            else
            {
                // Chosen property is no texture. All stays as in base class.
                base.OnGUI(position, property, label);
            }
        }

        // Draw material mode toggle
        EditorGUILayout.PropertyField(useMatP);
    }
}
