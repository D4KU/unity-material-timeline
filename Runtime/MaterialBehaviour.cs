using UnityEngine.Playables;
using UnityEngine;
using System;
using Spt = UnityEngine.Rendering.ShaderPropertyType;

/// <summary>
/// This base class exists because a <see cref="PropertyDrawer"/> can't
/// serialize generic types
/// </summary>
public abstract class MaterialBehaviourBase : PlayableBehaviour {}

public abstract class MaterialBehaviour<T> : MaterialBehaviourBase
{
    [Tooltip("The shader property to manipulate (e.g. '_BaseColor')")]
    public string propertyName;

    [Tooltip("New value of the shader property")]
    public T value;

    public MaterialBehaviour() : base() {}
    public MaterialBehaviour(MaterialBehaviour<T> source) : base()
    {
        propertyName = source.propertyName;
        value = source.value;
    }

    /// <summary>
    /// Apply this behaviour's value to the passed material
    /// </summary>
    public abstract void ToMaterial(T value, Material target);

    /// <summary>
    /// Set this behaviour's value from the given material
    /// </summary>
    public abstract T FromMaterial(Material source);

    /// <summary>
    /// Apply the linear interpolation of <param name="a"/> and
    /// <param name="b"/> to this behavior
    /// </summary>
    public abstract T Lerp(T a, T b, float t);

    protected abstract bool MatchesShaderProperty(Spt spt);

    /// <summary>
    /// Return true if the passed shader supports the property specified
    /// in this class
    /// </summary>
    protected bool ShaderHasProperty(Shader shader)
    {
        int propertyIndex = shader.FindPropertyIndex(propertyName);
        if (propertyIndex < 0)
            // Passed shader doesn't have any property with entered name
            return false;

        // Return true if found property has matching type
        return MatchesShaderProperty(shader.GetPropertyType(propertyIndex));
    }
}

[Serializable]
public class IntMaterialBehaviour : MaterialBehaviour<int>
{
    public override int FromMaterial(Material source)
        => source.GetInt(propertyName);

    public override void ToMaterial(int value, Material target)
        => target.SetInt(propertyName, value);

    public override int Lerp(int a, int b, float t)
        => (int)Mathf.Lerp(a, b, t);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Float || a == Spt.Range;
}

[Serializable]
public class FloatMaterialBehaviour : MaterialBehaviour<float>
{
    public override float FromMaterial(Material source)
        => source.GetFloat(propertyName);

    public override void ToMaterial(float value, Material target)
        => target.SetFloat(propertyName, value);

    public override float Lerp(float a, float b, float t)
        => Mathf.Lerp(a, b, t);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Float || a == Spt.Range;
}

[Serializable]
public class TextureMaterialBehaviour : MaterialBehaviour<Texture>
{
    public override Texture FromMaterial(Material source)
        => source.GetTexture(propertyName);

    public override void ToMaterial(Texture value, Material target)
        => target.SetTexture(propertyName, value);

    public override Texture Lerp(Texture a, Texture b, float t)
        => t < .5f? a : b;

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Texture;
}

[Serializable]
public class ColorMaterialBehaviour : MaterialBehaviour<Color>
{
    public override Color FromMaterial(Material source)
        => source.GetColor(propertyName);

    public override void ToMaterial(Color value, Material target)
        => target.SetColor(propertyName, value);

    public override Color Lerp(Color a, Color b, float t)
        => Color.Lerp(a, b, t);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Color || a == Spt.Vector;
}

[Serializable]
public class VectorMaterialBehaviour : MaterialBehaviour<Vector2>
{
    public override Vector2 FromMaterial(Material source)
        => source.GetVector(propertyName);

    public override void ToMaterial(Vector2 value, Material target)
        => target.SetVector(propertyName, value);

    public override Vector2 Lerp(Vector2 a, Vector2 b, float t)
        => Vector2.Lerp(a, b, t);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Color || a == Spt.Vector;
}

[Serializable]
public class ScaleMaterialBehaviour : VectorMaterialBehaviour
{
    public override Vector2 FromMaterial(Material source)
        => source.GetTextureScale(propertyName);

    public override void ToMaterial(Vector2 value, Material target)
        => target.SetTextureScale(propertyName, value);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Texture;
}

[Serializable]
public class OffsetMaterialBehaviour : VectorMaterialBehaviour
{
    public override Vector2 FromMaterial(Material source)
        => source.GetTextureOffset(propertyName);

    public override void ToMaterial(Vector2 value, Material target)
        => target.SetTextureOffset(propertyName, value);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Texture;
}

[Serializable]
public class MaterialMaterialBehaviour : MaterialBehaviour<Material>
{
    public override Material FromMaterial(Material source)
        => new Material(source);

    public override void ToMaterial(Material value, Material target)
    {
        if (value != null)
            target.CopyPropertiesFromMaterial(value);
    }

    public override Material Lerp(Material a, Material b, float t)
    {
        if (value != null && a != null && b != null)
            value.Lerp(a, b, t);
        return value;
    }

    protected override bool MatchesShaderProperty(Spt a)
        => false;
}
