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
    Color color;

    public Texture2D GetTexture(Color color)
    {
        bool wasClean = tex == null;

        if (wasClean)
            tex = new Texture2D(1, 1) { filterMode = FilterMode.Point };

        if (wasClean || color != this.color)
        {
            this.color = color;
            tex.SetPixel(0, 0, color);
            tex.Apply(updateMipmaps: true, makeNoLongerReadable: true);
        }
        return tex;
    }

    ~Texture2DCache()
    {
        if (tex)
            tex.SafeDestroy();
    }
}
}
