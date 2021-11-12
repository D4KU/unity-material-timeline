using UnityEngine;
using System;
using Spt = UnityEngine.Rendering.ShaderPropertyType;

namespace MaterialTrack
{
[Serializable]
public class MaterialBehaviour : RendererBehaviour
{
    public const string USE_MAT_FIELD = nameof(materialMode);
    public const string MAT_FIELD = nameof(material);

    [Tooltip("Override all properties of the bound material with the ones " +
            "found in this material")]
    public Material material;

    [Tooltip("Override all properties of the bound material with the ones " +
            "found in another one")]
    public bool materialMode;

    public MaterialBehaviour() : base() {}
    public MaterialBehaviour(MaterialBehaviour other) : base(other)
    {
        material = other.material;
        materialMode = other.materialMode;
    }

    /// <summary>
    /// Set this behaviour's value from the given material
    /// </summary>
    public override void ApplyFromMaterial(Material source)
    {
        if (materialMode)
            material = new Material(source);
        else
            base.ApplyFromMaterial(source);
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
                    case TextureTarget.TilingOffset:
                        target.SetTextureScale(propertyName, vector);
                        target.SetTextureOffset(
                            propertyName,
                            new Vector2(vector.z, vector.w));
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

    /// <inheritdoc cref="RendererBehaviour.IsBlendableWith"/>
    public override bool IsBlendableWith(RendererBehaviour other)
    {
        // If this behaviour is in material mode, it is only blendable
        // with another MaterialBehaviour in material mode.
        if (materialMode && other is MaterialBehaviour mb && mb.materialMode)
            return true;
        return base.IsBlendableWith(other);
    }
}
}
