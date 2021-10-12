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
    public void ApplyToMaterial(Material target)
    {
        if (ShaderHasProperty(target.shader))
            InnerApplyToMaterial(target);
    }

    /// <summary>
    /// Set this behaviour's value from the given material
    /// </summary>
    public void ApplyFromMaterial(Material source)
    {
        if (ShaderHasProperty(source.shader))
            value = InnerApplyFromMaterial(source);
    }

    /// <summary>
    /// Apply the linear interpolation of <param name="a"/> and
    /// <param name="b"/> to this behavior
    /// </summary>
    public T Lerp(MaterialBehaviour<T> a, MaterialBehaviour<T> b, float t)
        => value = InnerLerp(a.value, b.value, t);

    protected abstract void InnerApplyToMaterial(Material target);
    protected abstract T InnerApplyFromMaterial(Material source);
    protected abstract T InnerLerp(T a, T b, float t);
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
    protected override int InnerApplyFromMaterial(Material source)
        => source.GetInt(propertyName);

    protected override void InnerApplyToMaterial(Material target)
        => target.SetInt(propertyName, value);

    protected override int InnerLerp(int a, int b, float t)
        => (int)Mathf.Lerp(a, b, t);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Float || a == Spt.Range;
}

[Serializable]
public class FloatMaterialBehaviour : MaterialBehaviour<float>
{
    protected override float InnerApplyFromMaterial(Material source)
        => source.GetFloat(propertyName);

    protected override void InnerApplyToMaterial(Material target)
        => target.SetFloat(propertyName, value);

    protected override float InnerLerp(float a, float b, float t)
        => Mathf.Lerp(a, b, t);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Float || a == Spt.Range;
}

[Serializable]
public class TextureMaterialBehaviour : MaterialBehaviour<Texture>
{
    protected override Texture InnerApplyFromMaterial(Material source)
        => source.GetTexture(propertyName);

    protected override void InnerApplyToMaterial(Material target)
        => target.SetTexture(propertyName, value);

    protected override Texture InnerLerp(Texture a, Texture b, float t)
        => t < .5f? a : b;

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Texture;
}

[Serializable]
public class ColorMaterialBehaviour : MaterialBehaviour<Color>
{
    protected override Color InnerApplyFromMaterial(Material source)
        => source.GetColor(propertyName);

    protected override void InnerApplyToMaterial(Material target)
        => target.SetColor(propertyName, value);

    protected override Color InnerLerp(Color a, Color b, float t)
        => Color.Lerp(a, b, t);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Color || a == Spt.Vector;
}

[Serializable]
public class VectorMaterialBehaviour : MaterialBehaviour<Vector2>
{
    protected override Vector2 InnerApplyFromMaterial(Material source)
        => source.GetVector(propertyName);

    protected override void InnerApplyToMaterial(Material target)
        => target.SetVector(propertyName, value);

    protected override Vector2 InnerLerp(Vector2 a, Vector2 b, float t)
        => Vector2.Lerp(a, b, t);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Color || a == Spt.Vector;
}

[Serializable]
public class ScaleMaterialBehaviour : VectorMaterialBehaviour
{
    protected override Vector2 InnerApplyFromMaterial(Material source)
        => source.GetTextureScale(propertyName);

    protected override void InnerApplyToMaterial(Material target)
        => target.SetTextureScale(propertyName, value);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Texture;
}

[Serializable]
public class OffsetMaterialBehaviour : VectorMaterialBehaviour
{
    protected override Vector2 InnerApplyFromMaterial(Material source)
        => source.GetTextureOffset(propertyName);

    protected override void InnerApplyToMaterial(Material target)
        => target.SetTextureOffset(propertyName, value);

    protected override bool MatchesShaderProperty(Spt a)
        => a == Spt.Texture;
}

[Serializable]
public class MaterialMaterialBehaviour : MaterialBehaviour<Material>
{
    protected override Material InnerApplyFromMaterial(Material source)
        => new Material(source);

    protected override void InnerApplyToMaterial(Material target)
    {
        if (value != null)
            target.CopyPropertiesFromMaterial(value);
    }

    protected override Material InnerLerp(Material a, Material b, float t)
    {
        if (value != null && a != null && b != null)
            value.Lerp(a, b, t);
        return value;
    }

    protected override bool MatchesShaderProperty(Spt a)
        => false;
}
