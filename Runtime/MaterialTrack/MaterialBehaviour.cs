﻿using UnityEngine;
using System;
using UnityEngine.Rendering;
using Spt = UnityEngine.Rendering.ShaderPropertyType;

namespace MaterialTrack
{
[Serializable]
public class MaterialBehaviour : RendererBehaviour
{
    public const string TEX_TARGET_FIELD = nameof(textureTarget);
    public const string USE_MAT_FIELD = nameof(materialMode);
    public const string MAT_FIELD = nameof(material);

    /// <summary>
    /// Specifies how a texture is manipulated
    /// </summary>
    public enum TextureTarget
    {
        Asset,
        Tiling,
        Offset
    }

    [Tooltip("Override all properties of the bound material with the ones " +
            "found in this material")]
    public Material material;

    [Tooltip("Override all properties of the bound material with the ones " +
            "found in another one")]
    public bool materialMode;

    [Tooltip("How to manipulate the texture?")]
    public TextureTarget textureTarget;

    public MaterialBehaviour() : base() {}
    public MaterialBehaviour(MaterialBehaviour other) : base(other)
    {
        material = other.material;
        materialMode = other.materialMode;
        textureTarget = other.textureTarget;
    }

    /// <summary>
    /// Set this behaviour's value from the given material
    /// </summary>
    new public void ApplyFromMaterial(Material source)
    {
        if (materialMode)
        {
            material = new Material(source);
            return;
        }

        if (!HasProperty(source))
            return;

        switch (propertyType)
        {
            case Spt.Float:
            case Spt.Range:
                vector.x = source.GetFloat(propertyName);
                break;
            case Spt.Texture:
                switch (textureTarget)
                {
                    case TextureTarget.Asset:
                        texture = source.GetTexture(propertyName);
                        break;
                    case TextureTarget.Tiling:
                        vector = source.GetTextureScale(propertyName);
                        break;
                    case TextureTarget.Offset:
                        vector = source.GetTextureOffset(propertyName);
                        break;
                }
                break;
            default:
                vector = source.GetVector(propertyName);
                break;
        }
    }

    /// <summary>
    /// Apply this behaviour's value to the passed material
    /// </summary>
    public void ApplyToMaterial(Material target)
    {
        if (materialMode)
        {
            if (material && material != target)
                target.CopyPropertiesFromMaterial(material);
            return;
        }

        if (!HasProperty(target))
            return;

        switch (propertyType)
        {
            case Spt.Float:
            case Spt.Range:
                target.SetFloat(propertyName, vector.x);
                break;
            case Spt.Texture:
                switch (textureTarget)
                {
                    case TextureTarget.Asset:
                        ApplyToTexture(target);
                        break;
                    case TextureTarget.Tiling:
                        target.SetTextureScale(propertyName, vector);
                        break;
                    case TextureTarget.Offset:
                        target.SetTextureOffset(propertyName, vector);
                        break;
                }
                break;
            default:
                target.SetVector(propertyName, vector);
                break;
        }
    }

    /// <summary>
    /// Interpret this behaviour's data as texture property data and set it
    /// in the given material
    /// </summary>
    void ApplyToTexture(Material target)
    {
        // Create a 2D texture from the set default color if behaviour has
        // no texture set
        if (!texture)
            texture = vector.ToTexture2D();

        if (texture.dimension ==
                target.shader.GetPropertyTextureDimension(propertyName))
            target.SetTexture(propertyName, texture);
    }

    /// <summary>
    /// Apply the linear interpolation of <param name="a"/> and
    /// <param name="b"/> to this behaviour
    /// </summary>
    public void Lerp(MaterialBehaviour a, MaterialBehaviour b, float t)
    {
        if (materialMode)
        {
            if (material != null && a.material != null && b.material != null)
                material.Lerp(a.material, b.material, t);
        }
        else
        {
            base.Lerp(a, b, t);
        }
    }
}
}
