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
}
}
