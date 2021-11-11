using UnityEngine;
using UnityEngine.Rendering;

namespace MaterialTrack
{
public static class ExtensionMethods
{
    public static TextureDimension
        GetPropertyTextureDimension(this Shader shader, string propertyName)
    {
        int propIdx = shader.FindPropertyIndex(propertyName);
        if (propIdx < 0)
            // Shader doesn't have any property with given name
            return TextureDimension.Unknown;
        return shader.GetPropertyTextureDimension(propIdx);
    }

    /// <summary>
    /// Copy the given render texture into a new equally sized Texture2D
    /// </summary>
    public static Texture2D ToTexture2D(this RenderTexture source)
    {
        // Create result texture of identical size
        int width = source.width;
        int height = source.height;
        Texture2D result = new Texture2D(width, height);

        // Save state before this function ran
        RenderTexture originalActive = RenderTexture.active;

        // Prepare global render state to read render texture
        RenderTexture.active = source;

        // Copy over pixels.
        // Can't use faster Graphics.CopyTexture() because it expects
        // equal mipmap counts.
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        // Re-apply original state
        RenderTexture.active = originalActive;

        // Send texture to GPU
        result.Apply(updateMipmaps: true, makeNoLongerReadable: true);
        return result;
    }

    /// <summary>
    /// Creates a 1x1 texture in the given color
    /// </summary>
    public static Texture2D ToTexture2D(this Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);

        // Send texture to GPU
        tex.Apply(updateMipmaps: true, makeNoLongerReadable: true);
        return tex;
    }

    /// <summary>
    /// Interpret the vector as color and create a 1x1 texture in this color
    /// </summary>
    public static Texture2D ToTexture2D(this Vector4 vector)
        => ((Color)vector).ToTexture2D();

    public static Vector4 GetTextureScaleOffset(
            this MaterialPropertyBlock block, string name)
        => block.GetVector(name + "_ST");

    public static void SetTextureScaleOffset(
            this MaterialPropertyBlock block, string name, Vector4 value)
        => block.SetVector(name + "_ST", value);

    public static Vector2 GetTextureScale(
            this MaterialPropertyBlock block, string name)
        => block.GetTextureScaleOffset(name);

    public static void SetTextureScale(
            this MaterialPropertyBlock block, string name, Vector2 value)
        => block.SetTextureScaleOffset(name, new Vector4(value.x, value.y, 0, 0));

    public static Vector2 GetTextureOffset(
            this MaterialPropertyBlock block, string name)
    {
        Vector4 scaleOffset = block.GetTextureScaleOffset(name);
        return new Vector2(scaleOffset.z, scaleOffset.w);
    }

    public static void SetTextureOffset(
            this MaterialPropertyBlock block, string name, Vector2 value)
        => block.SetTextureScaleOffset(name, new Vector4(1, 1, value.x, value.y));

    public static Color TextureDefaultNameToColor(this string name)
        => name.ToLowerInvariant() switch
        {
            "black" => Color.black,
            "grey"  => Color.grey,
            "bump"  => new Color(.5f, .5f, 1f, 1f),
            _       => Color.white,
        };
}
}
