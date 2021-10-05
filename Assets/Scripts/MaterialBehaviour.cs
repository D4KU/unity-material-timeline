using UnityEngine.Playables;
using UnityEngine;
using System;

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

    public MaterialBehaviour(){}
    public MaterialBehaviour(MaterialBehaviour source)
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
        switch (propertyType)
        {
            case PropertyType.Int:
                intValue = source.GetInt(propertyName);
                break;
            case PropertyType.Float:
                floatValue = source.GetFloat(propertyName);
                break;
            case PropertyType.Texture:
                texture = source.GetTexture(propertyName);
                break;
            case PropertyType.Color:
                color = source.GetColor(propertyName);
                break;
            case PropertyType.TextureTiling:
                tiling = source.GetTextureScale(propertyName);
                break;
            case PropertyType.TextureOffset:
                offset = source.GetTextureOffset(propertyName);
                break;
            case PropertyType.Vector:
                vector = source.GetVector(propertyName);
                break;
            case PropertyType.Material:
                material = new Material(source);
                break;
        }
    }

    public void ApplyToMaterial(Material target)
    {
        switch (propertyType)
        {
            case PropertyType.Int:
                target.SetInt(propertyName, intValue);
                break;
            case PropertyType.Float:
                target.SetFloat(propertyName, floatValue);
                break;
            case PropertyType.Color:
                target.SetColor(propertyName, color);
                break;
            case PropertyType.Texture:
                target.SetTexture(propertyName, texture);
                break;
            case PropertyType.TextureTiling:
                target.SetTextureScale(propertyName, tiling);
                break;
            case PropertyType.TextureOffset:
                target.SetTextureOffset(propertyName, offset);
                break;
            case PropertyType.Vector:
                target.SetVector(propertyName, vector);
                break;
            case PropertyType.Material:
                if (material != null)
                    target.CopyPropertiesFromMaterial(material);
                break;
        }
    }

    public void Lerp(MaterialBehaviour a, MaterialBehaviour b, float t)
    {
        switch (propertyType)
        {
            case PropertyType.Int:
                intValue = (int)Mathf.Lerp(a.intValue, b.intValue, t);
                break;
            case PropertyType.Float:
                floatValue = Mathf.Lerp(a.floatValue, b.floatValue, t);
                break;
            case PropertyType.Texture:
                texture = a.texture;
                break;
            case PropertyType.Color:
                color = Color.Lerp(a.color, b.color, t);
                break;
            case PropertyType.TextureTiling:
                tiling = Vector2.Lerp(a.tiling, b.tiling, t);
                break;
            case PropertyType.TextureOffset:
                offset = Vector2.Lerp(a.offset, b.offset, t);
                break;
            case PropertyType.Vector:
                vector = Vector4.Lerp(a.vector, b.vector, t);
                break;
            case PropertyType.Material:
                if (material != null && a.material != null && b.material != null)
                    material.Lerp(a.material, b.material, t);
                break;
        }
    }
}
