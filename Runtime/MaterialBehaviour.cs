using UnityEngine.Playables;
using UnityEngine;
using System;
using Spt = UnityEngine.Rendering.ShaderPropertyType;

[Serializable]
public class MaterialBehaviour : PlayableBehaviour
{
    private const string TOOLTIP = "New value of the shader property";

    public RendererMixer mixer;

    [Tooltip("The shader property to manipulate (e.g. '_BaseColor')")]
    public string propertyName = "";

    [Tooltip("Type of the shader property to manipulate")]
    public Spt propertyType = Spt.Float;

    [Tooltip(TOOLTIP)]
    public float number;

    [Tooltip(TOOLTIP)]
    public Texture texture;

    [Tooltip(TOOLTIP)]
    public Color color;

    [Tooltip(TOOLTIP)]
    public Vector4 vector;

    [Tooltip("Override all properties of the bound material with the ones " +
            "found in this material")]
    public Material material;

    public string FieldName => propertyType switch
    {
        Spt.Color   => nameof(color),
        Spt.Texture => nameof(texture),
        Spt.Vector  => nameof(vector),
        _           => nameof(number)
    };

    public MaterialBehaviour() : base() {}

    public MaterialBehaviour(MaterialBehaviour source) : base()
    {
        propertyName = source.propertyName;
        propertyType = source.propertyType;
        number = source.number;
        texture = source.texture;
        color = source.color;
        vector = source.vector;
        material = source.material;
    }

    /// <summary>
    /// Set this behaviour's value from the given material
    /// </summary>
    public void ApplyFromMaterial(Material source)
    {
        if (!source.HasProperty(Shader.PropertyToID(propertyName)))
            return;

        switch (propertyType)
        {
            case Spt.Float:
            case Spt.Range:
                number = source.GetFloat(propertyName);
                break;
            case Spt.Texture:
                texture = source.GetTexture(propertyName);
                break;
            case Spt.Color:
                color = source.GetColor(propertyName);
                break;
            case Spt.Vector:
                vector = source.GetVector(propertyName);
                break;
            default:
                throw new ArgumentException();
            // case Spt.Material:
            //     material = new Material(source);
            //     break;
        }
    }

    /// <summary>
    /// Apply this behaviour's value to the passed material
    /// </summary>
    public void ApplyToMaterial(Material target)
    {
        if (!target.HasProperty(Shader.PropertyToID(propertyName)))
            return;

        switch (propertyType)
        {
            case Spt.Float:
            case Spt.Range:
                target.SetFloat(propertyName, number);
                break;
            case Spt.Color:
                target.SetColor(propertyName, color);
                break;
            case Spt.Texture:
                if (texture != null)
                    target.SetTexture(propertyName, texture);
                break;
            case Spt.Vector:
                target.SetVector(propertyName, vector);
                break;
            default:
                throw new ArgumentException();
            // case Spt.Material:
            //     if (material != null)
            //         target.CopyPropertiesFromMaterial(material);
            //     break;
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
            case Spt.Float:
            case Spt.Range:
                number = Mathf.Lerp(a.number, b.number, t);
                break;
            case Spt.Texture:
                texture = a.texture;
                break;
            case Spt.Color:
                color = Color.Lerp(a.color, b.color, t);
                break;
            case Spt.Vector:
                vector = Vector4.Lerp(a.vector, b.vector, t);
                break;
            default:
                throw new ArgumentException();
            // case Spt.Material:
            //     if (material != null && a.material != null && b.material != null)
            //         material.Lerp(a.material, b.material, t);
            //     break;
        }
    }

    public void ApplyFromPropertyBlock(MaterialPropertyBlock source)
    {
        switch (propertyType)
        {
            case Spt.Float:
            case Spt.Range:
                number = source.GetFloat(propertyName);
                break;
            case Spt.Texture:
                texture = source.GetTexture(propertyName);
                break;
            case Spt.Color:
                color = source.GetColor(propertyName);
                break;
            case Spt.Vector:
                vector = source.GetVector(propertyName);
                break;
            default:
                throw new ArgumentException();
        }
    }

    public void ApplyToPropertyBlock(MaterialPropertyBlock target)
    {
        switch (propertyType)
        {
            case Spt.Float:
            case Spt.Range:
                target.SetFloat(propertyName, number);
                break;
            case Spt.Color:
                target.SetColor(propertyName, color);
                break;
            case Spt.Texture:
                if (texture != null)
                    target.SetTexture(propertyName, texture);
                break;
            case Spt.Vector:
                target.SetVector(propertyName, vector);
                break;
            default:
                throw new ArgumentException();
        }
    }
}
