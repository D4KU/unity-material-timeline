using UnityEngine;

namespace MaterialTrack
{
/// <summary>
/// Provides a 1x1 <see cref="Texture2D"/> in a requested color.
/// The same texture is recycled upon each request.
/// </summary>
public class Texture2DCache
{
    Texture2D tex;

    public Texture2D GetTexture(Color color)
    {
        if (tex == null)
            tex = new Texture2D(1, 1);

        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    ~Texture2DCache()
    {
        if (tex)
            tex.SafeDestroy();
    }
}
}
