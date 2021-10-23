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
                var valLabel = new GUIContent("Value");
                var texTrgP = property.FindPropertyRelative(T.TEX_TARGET_FIELD);
                EditorGUILayout.PropertyField(texTrgP);

                if (texTrgP.enumValueIndex == (int)T.TextureTarget.Asset)
                {
                    var texP = property.FindPropertyRelative(T.TEX_FIELD);
                    EditorGUILayout.PropertyField(texP, valLabel);
                }
                else
                {
                    var vecP = property.FindPropertyRelative(T.VEC_FIELD);
                    vecP.vector4Value =
                        EditorGUILayout.Vector2Field(valLabel, vecP.vector4Value);
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
