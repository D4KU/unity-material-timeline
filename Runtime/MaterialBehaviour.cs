using UnityEngine.Playables;
using UnityEngine;
using System;
using Spt = UnityEngine.Rendering.ShaderPropertyType;
using Pt = MaterialBehaviour.PropertyType;

// TODO Only store the values used.
// Create a clip for each property type?
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

    public string propertyName;
    public PropertyType propertyType;
    public int intValue;
    public float floatValue;
    public Texture texture;
    public Vector2 tiling;
    public Vector2 offset;
    public Color color;
    public Vector4 vector;
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

    bool ShaderHasProperty(Shader shader)
    {
        int propertyIndex = shader.FindPropertyIndex(propertyName);
        if (propertyIndex < 0)
            // Passed material doesn't have any property with entered name
            return false;

        // Return if found property has matching type
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
