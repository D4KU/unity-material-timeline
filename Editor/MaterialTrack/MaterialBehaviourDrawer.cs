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
        var useMatP = property.FindPropertyRelative(T.USE_MAT_FIELD);
        return useMatP.boolValue ? 0f : base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        var useMatP = property.FindPropertyRelative(T.USE_MAT_FIELD);
        if (useMatP.boolValue)
        {
            var matProp = property.FindPropertyRelative(T.MAT_FIELD);
            EditorGUILayout.PropertyField(matProp);
        }
        else
        {
            DrawPropertyDropdown(position, property);

            var typeP = property.FindPropertyRelative(T.TYPE_FIELD);
            if (typeP.enumValueIndex == (int)ShaderPropertyType.Texture)
            {
                var texTrgP = property.FindPropertyRelative(T.TEX_TARGET_FIELD);
                EditorGUILayout.PropertyField(texTrgP);

                if (texTrgP.enumValueIndex == (int)T.TextureTarget.Asset)
                {
                    DrawTextureField(property);
                }
                else
                {
                    var vecP = property.FindPropertyRelative(T.VEC_FIELD);
                    Vector2 vec2 =
                        EditorGUILayout.Vector2Field(ValueLabel, vecP.vector4Value);
                    Vector4 vec4 = vecP.vector4Value;
                    vecP.vector4Value = new Vector4(vec2.x, vec2.y, vec4.z, vec4.w);
                }
            }
            else
            {
                base.OnGUI(position, property, label);
            }
        }

        EditorGUILayout.PropertyField(useMatP);
    }
}
