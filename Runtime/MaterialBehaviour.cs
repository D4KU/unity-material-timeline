using UnityEngine.Playables;
using UnityEngine;
using System;
using Spt = UnityEngine.Rendering.ShaderPropertyType;
using Pt = MaterialBehaviour.PropertyType;

[Serializable]
public class MaterialBehaviour : PlayableBehaviour
{
    public enum PropertyType
    {
        Int,
        Float,
        Texture,
        TextureTiling,
        TextureOffset,
        Color,
        Vector,
        Material,
    }
    private const string TOOLTIP = "New value of the shader property";

    [Tooltip("The shader property to manipulate (e.g. '_BaseColor')")]
    public string propertyName = "";

    [Tooltip("Type of the shader property to manipulate")]
    public PropertyType propertyType;

    [Tooltip(TOOLTIP)]
    public int intValue;

    [Tooltip(TOOLTIP)]
    public float floatValue;

    [Tooltip(TOOLTIP)]
    public Texture texture;

    [Tooltip(TOOLTIP)]
    public Vector2 tiling;

    [Tooltip(TOOLTIP)]
    public Vector2 offset;

    [Tooltip(TOOLTIP)]
    public Color color;

    [Tooltip(TOOLTIP)]
    public Vector4 vector;

    [Tooltip("Override all properties of the bound material with the ones " +
            "found in this material")]
    public Material material;

    public MaterialBehaviour() : base() {}

    public MaterialBehaviour(MaterialBehaviour source) : base()
    {
        propertyName = source.propertyName;
        propertyType = source.propertyType;
        intValue = source.intValue;
        floatValue = source.floatValue;
        texture = source.texture;
        color = source.color;
        tiling = source.tiling;
        offset = source.offset;
        vector = source.vector;
        material = source.material;
    }

    /// <summary>
    /// Set this behaviour's value from the given material
    /// </summary>
    public void ApplyFromMaterial(Material source)
    {
        if (!ShaderHasProperty(source.shader))
            return;

        switch (propertyType)
        {
            case Pt.Int:
                intValue = source.GetInt(propertyName);
                break;
            case Pt.Float:
                floatValue = source.GetFloat(propertyName);
                break;
            case Pt.Texture:
                texture = source.GetTexture(propertyName);
                break;
            case Pt.Color:
                color = source.GetColor(propertyName);
                break;
            case Pt.TextureTiling:
                tiling = source.GetTextureScale(propertyName);
                break;
            case Pt.TextureOffset:
                offset = source.GetTextureOffset(propertyName);
                break;
            case Pt.Vector:
                vector = source.GetVector(propertyName);
                break;
            case Pt.Material:
                material = new Material(source);
                break;
        }
    }

    /// <summary>
    /// Apply this behaviour's value to the passed material
    /// </summary>
    public void ApplyToMaterial(Material target)
    {
        if (!ShaderHasProperty(target.shader))
            return;

        switch (propertyType)
        {
            case Pt.Int:
                target.SetInt(propertyName, intValue);
                break;
            case Pt.Float:
                target.SetFloat(propertyName, floatValue);
                break;
            case Pt.Color:
                target.SetColor(propertyName, color);
                break;
            case Pt.Texture:
                target.SetTexture(propertyName, texture);
                break;
            case Pt.TextureTiling:
                target.SetTextureScale(propertyName, tiling);
                break;
            case Pt.TextureOffset:
                target.SetTextureOffset(propertyName, offset);
                break;
            case Pt.Vector:
                target.SetVector(propertyName, vector);
                break;
            case Pt.Material:
                if (material != null)
                    target.CopyPropertiesFromMaterial(material);
                break;
        }
    }

    /// <summary>
    /// Apply the linear interpolation of <param name="a"/> and
    /// <param name="b"/> to this behaviour
    /// </summary>
    public void Lerp(MaterialBehaviour a, MaterialBehaviour b, float t)
    {
        switch (propertyType)
        {
            case Pt.Int:
                intValue = (int)Mathf.Lerp(a.intValue, b.intValue, t);
                break;
            case Pt.Float:
                floatValue = Mathf.Lerp(a.floatValue, b.floatValue, t);
                break;
            case Pt.Texture:
                texture = a.texture;
                break;
            case Pt.Color:
                color = Color.Lerp(a.color, b.color, t);
                break;
            case Pt.TextureTiling:
                tiling = Vector2.Lerp(a.tiling, b.tiling, t);
                break;
            case Pt.TextureOffset:
                offset = Vector2.Lerp(a.offset, b.offset, t);
                break;
            case Pt.Vector:
                vector = Vector4.Lerp(a.vector, b.vector, t);
                break;
            case Pt.Material:
                if (material != null && a.material != null && b.material != null)
                    material.Lerp(a.material, b.material, t);
                break;
        }
    }

    /// <summary>
    /// Return true if the passed shader supports the property specified
    /// in this class
    /// </summary>
    private bool ShaderHasProperty(Shader shader)
    {
        int propertyIndex = shader.FindPropertyIndex(propertyName);
        if (propertyIndex < 0)
            // Passed shader doesn't have any property with entered name
            return false;

        // Return true if found property has matching type
        var t = shader.GetPropertyType(propertyIndex);
        return propertyType switch
        {
            Pt.Int           => t == Spt.Float || t == Spt.Range,
            Pt.Float         => t == Spt.Float || t == Spt.Range,
            Pt.Color         => t == Spt.Color || t == Spt.Vector,
            Pt.Vector        => t == Spt.Color || t == Spt.Vector,
            Pt.Texture       => t == Spt.Texture,
            Pt.TextureTiling => t == Spt.Texture,
            Pt.TextureOffset => t == Spt.Texture,
            _                => false,
        };
    }
}
