using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

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

    public static Vector4 GetTextureScaleOffset(
            this MaterialPropertyBlock block, string name)
        => block.GetVector(name + "_ST");

    public static void SetTextureScaleOffset(
            this MaterialPropertyBlock block, string name, Vector4 value)
        => block.SetVector(name + "_ST", value);

    public static Color TextureDefaultNameToColor(this string name)
        => name.ToLowerInvariant() switch
        {
            "black" => Color.black,
            "grey"  => Color.grey,
            "bump"  => new Color(.5f, .5f, 1f, 1f),
            _       => Color.white,
        };

    public static bool TryGetBinding<T>(
        this TrackAsset track,
        GameObject owner,
        out T binding) where T : class
    {
        var key = track.isSubTrack ? track.parent : track;
        binding = owner.GetComponent<PlayableDirector>()
            .GetGenericBinding(key) as T;
        return binding != null;
    }

    public static void ResizeArray<T>(
        ref T[] array,
        int newSize,
        T defaultValue = default)
    {
        int oldSize = 0;
        if (array == null)
            array = new T[newSize];
        else
        {
            oldSize = array.Length;
            if (newSize == oldSize)
                return;
            Array.Resize(ref array, newSize);
        }

        for (int i = Math.Max(oldSize - 1, 0); i < newSize; i++)
            array[i] = defaultValue;
    }

    /// <summary>
    /// Destroy working inside and outside Play mode
    /// </summary>
    public static void SafeDestroy(this UnityEngine.Object o)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEngine.Object.DestroyImmediate(o);
        else
#endif
            UnityEngine.Object.Destroy(o);
    }
}
}
