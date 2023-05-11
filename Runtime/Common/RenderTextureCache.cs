using UnityEngine;

namespace MaterialTrack
{
/// <summary>
/// Provides a <see cref="RenderTexture"/> resized to a requested resolution.
/// The same texture is recycled upon each request.
/// </summary>
public class RenderTextureCache
{
    RenderTexture rt;

    public RenderTexture GetTexture(int width, int height)
    {
        if (rt == null)
        {
            rt = new RenderTexture(width, height, 0);
        }
        else if (width != rt.width || height != rt.height)
        {
            rt.Release();
            rt.width = width;
            rt.height = height;
        }

        return rt;
    }

    ~RenderTextureCache()
    {
        if (rt)
            rt.SafeDestroy();
    }
}
}
