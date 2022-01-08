using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using Spt = UnityEngine.Rendering.ShaderPropertyType;

namespace MaterialTrack
{
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
    public const string TEX_TARGET_FIELD = nameof(textureTarget);

    /// <summary>
    /// Specifies how a texture is manipulated
    /// </summary>
    public enum TextureTarget
    {
        Asset,
        TilingOffset
    }

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

    [Tooltip("How to manipulate the texture?")]
    public TextureTarget textureTarget;

    /// <inheritdoc cref="IMaterialProvider.Materials"/>
    public IEnumerable<Material> Materials => provider?.Materials;

    /// To not search for a shader every frame that we'll never find.
    /// Only considered in builds.
#if !UNITY_EDITOR
    static bool triedFindShader;
#endif

    /// <inheritdoc cref="BlendMaterial"/>
    static Material blendMaterial;

    /// Material used to blend textures
    public static Material BlendMaterial
    {
        get
        {
            if (blendMaterial == null
#if !UNITY_EDITOR
            && !triedFindShader
#endif
                )
            {
                Shader shader = Shader.Find("Hidden/MaterialTrack/TextureBlend");
                if (shader == null)
                    Debug.LogWarning("'TextureBlend' shader could not be found. " +
                        "To ensure it's included in the build, add it to the " +
                        "list of always included shaders under ProjectSettings " +
                        "> Graphics.");
                else
                    blendMaterial = new Material(shader);
#if !UNITY_EDITOR
                triedFindShader = true;
#endif
            }
            return blendMaterial;
        }
    }

    public RendererBehaviour() : base() {}
    public RendererBehaviour(RendererBehaviour other) : base()
    {
        propertyName  = other.propertyName;
        propertyType  = other.propertyType;
        texture       = other.texture;
        vector        = other.vector;
        textureTarget = other.textureTarget;
    }

    protected bool HasProperty(Material material)
        => material.HasProperty(Shader.PropertyToID(propertyName));

    /// <summary>
    /// Apply the linear interpolation of <param name="a"/> and
    /// <param name="b"/> to this behaviour
    /// </summary>
    public virtual void Lerp(RendererBehaviour a, RendererBehaviour b, float t)
    {
        if (propertyType == Spt.Texture && textureTarget == TextureTarget.Asset)
        {
            Texture texA = a.texture ? a.texture : a.vector.ToTexture2D();
            Texture texB = b.texture ? b.texture : b.vector.ToTexture2D();
            texture = BlendTextures(texA, texB, t);

            // If blend failed, resort to hard cut
            if (texture == null)
                texture = t < .5f ? texA : texB;
        }
        // Lerp vector even if target is a texture asset, because it is then
        // used to store the texture default value.
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
                switch (textureTarget)
                {
                    case TextureTarget.Asset:
                        texture = source.GetTexture(propertyName);
                        break;
                    case TextureTarget.TilingOffset:
                        vector = source.GetTextureScaleOffset(propertyName);
                        break;
                }
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
                switch (textureTarget)
                {
                    case TextureTarget.Asset:
                        if (texture == null)
                            texture = vector.ToTexture2D();
                        target.SetTexture(propertyName, texture);
                        break;
                    case TextureTarget.TilingOffset:
                        target.SetTextureScaleOffset(propertyName, vector);
                        break;
                }
                break;
            default:
                target.SetVector(propertyName, vector);
                break;
        }
    }

    /// <summary>
    /// Set this behaviour's value from the given material
    /// </summary>
    public virtual void ApplyFromMaterial(Material source)
    {
        if (!HasProperty(source))
            return;

        Shader shader = source.shader;
        int propIndex  = shader.FindPropertyIndex(propertyName);
        switch (propertyType)
        {
            case Spt.Float:
                vector.x = source.GetFloat(propertyName);
                break;
            case Spt.Range:
                Vector2 limits = shader.GetPropertyRangeLimits(propIndex);

                // Pack range limits into unused vector components
                vector.x = source.GetFloat(propertyName);
                vector.y = limits.x;
                vector.z = limits.y;
                break;
            case Spt.Texture:
                switch (textureTarget)
                {
                    case TextureTarget.Asset:
                        texture = source.GetTexture(propertyName);
                        // default is a white texture mode
                        string defaultName =
                            shader.GetPropertyTextureDefaultName(propIndex);
                        vector = defaultName.TextureDefaultNameToColor();
                        break;
                    case TextureTarget.TilingOffset:
                        Vector2 scale = source.GetTextureScale(propertyName);
                        Vector2 offset = source.GetTextureOffset(propertyName);
                        vector = new Vector4(scale.x, scale.y, offset.x, offset.y);
                        break;
                }
                break;
            default:
                vector = source.GetVector(propertyName);
                break;
        }
    }

    /// <summary>
    /// Create a new texture as linear interpolation of the given textures.
    /// Resolution is also interpolated.
    /// The first texture passed determines the output texture's non-blendable
    /// properties, such as wrapMode.
    /// </summary>
    protected Texture BlendTextures(Texture a, Texture b, float t)
    {
        if (a.dimension != TextureDimension.Tex2D ||
            b.dimension != TextureDimension.Tex2D ||
            BlendMaterial == null)
            return null;

        // Set 'b' and 't' in material for blending.
        // 'a' is set by Graphics.Blit() to the '_MaintTex' property.
        blendMaterial.SetTexture("_SideTex", b);
        blendMaterial.SetFloat("_Weight", t);

        RenderTexture result = new RenderTexture(
            width:  (int)Mathf.Lerp(a.width,  b.width,  t),
            height: (int)Mathf.Lerp(a.height, b.height, t),
            depth: 0)
        {
            anisoLevel = (int)Mathf.Lerp(a.anisoLevel, b.anisoLevel, t),
            filterMode = (FilterMode)Mathf.Lerp((int)a.filterMode, (int)b.filterMode, t),
            wrapMode   = a.wrapMode,
        };

        // Render blend of both given textures to render texture.
        Graphics.Blit(a, result, blendMaterial);
        return result;
    }

    /// <summary>
    /// Returns true if it is possible to lerp between this and the given
    /// behaviour.
    /// </summary>
    public virtual bool IsBlendableWith(RendererBehaviour other)
        => other != null
        && propertyName == other.propertyName
        && propertyType == other.propertyType
        && (propertyType != Spt.Texture || textureTarget == other.textureTarget);
        // If two behaviours aren't textures, they are blendable if their
        // property names and types match. If they are textures, they must
        // also have identical texture targets.
}
}
