using UnityEngine.Playables;
using UnityEngine;
using System;
using Spt = UnityEngine.Rendering.ShaderPropertyType;
using System.Collections.Generic;

/// <summary>
/// The data container of each clip. It can be seen as a <i>tagged union</i>,
/// where the tag is the shader property name of the active value. Because
/// a shader property can have different types, there are several value
/// fields, but only one field is considered active at a time.
/// </summary>
[Serializable]
public class RendererBehaviour : PlayableBehaviour, IMaterialProvider
{
    /// Used for serialization in this class's inspector drawer
    public const string TYPE_FIELD = nameof(propertyType);
    public const string NAME_FIELD = nameof(propertyName);
    public const string TEX_FIELD = nameof(texture);
    public const string VEC_FIELD = nameof(vector);

    /// <summary>
    /// Object providing the manipulated materials. Set from the outside.
    /// </summary>
    public IMaterialProvider provider;

    [Tooltip("Name of the shader property to manipulate")]
    public string propertyName = "";

    [Tooltip("Type of the shader property to manipulate")]
    public Spt propertyType = Spt.Float;

    [Tooltip("Texture to assign to the shader property")]
    public Texture texture;

    [Tooltip("Value to assign to the shader property")]
    public Vector4 vector;

    /// <inheritdoc cref="IMaterialProvider.Materials"/>
    public IEnumerable<Material> Materials => provider?.Materials;

    public RendererBehaviour() : base() {}
    public RendererBehaviour(RendererBehaviour other) : base()
    {
        propertyName = other.propertyName;
        propertyType = other.propertyType;
        texture      = other.texture;
        vector       = other.vector;
    }

    protected bool HasProperty(Material material)
        => material.HasProperty(Shader.PropertyToID(propertyName));

    /// <summary>
    /// Apply the linear interpolation of <param name="a"/> and
    /// <param name="b"/> to this behaviour
    /// </summary>
    public virtual void Lerp(RendererBehaviour a, RendererBehaviour b, float t)
    {
        texture = a.texture;
        vector = Vector4.Lerp(a.vector, b.vector, t);
    }

    /// <summary>
    /// Set this behaviour's value from the given material property block
    /// </summary>
    public void ApplyFromPropertyBlock(MaterialPropertyBlock source)
    {
        switch (propertyType)
        {
            case Spt.Float:
            case Spt.Range:
                vector.x = source.GetFloat(propertyName);
                break;
            case Spt.Texture:
                texture = source.GetTexture(propertyName);
                break;
            default:
                vector = source.GetVector(propertyName);
                break;
        }
    }

    /// <summary>
    /// Apply this behaviour's value to the given material property block
    /// </summary>
    public void ApplyToPropertyBlock(MaterialPropertyBlock target)
    {
        switch (propertyType)
        {
            case Spt.Float:
            case Spt.Range:
                target.SetFloat(propertyName, vector.x);
                break;
            case Spt.Texture:
                if (texture != null)
                    target.SetTexture(propertyName, texture);
                break;
            default:
                target.SetVector(propertyName, vector);
                break;
        }
    }

    /// <summary>
    /// Set this behaviour's value from the given material
    /// </summary>
    public void ApplyFromMaterial(Material source)
    {
        if (!HasProperty(source))
            return;

        switch (propertyType)
        {
            case Spt.Float:
            case Spt.Range:
                vector.x = source.GetFloat(propertyName);
                break;
            case Spt.Texture:
                texture = source.GetTexture(propertyName);
                break;
            default:
                vector = source.GetVector(propertyName);
                break;
        }
    }
}

