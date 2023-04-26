using UnityEngine;

namespace MaterialTrack
{
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
