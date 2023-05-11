using UnityEngine;
using UnityEngine.Playables;
using System.Linq;
using System.Collections.Generic;

namespace MaterialTrack
{
public class MaterialMixer : PlayableBehaviour, IMixer
{
    /// <summary>
    /// Material manipulated by the track
    /// </summary>
    Material boundMaterial;

    /// <summary>
    /// Material state before timeline initialized
    /// </summary>
    Material defaultMaterial;

    /// <summary>
    /// Initialization helper
    /// </summary>
    bool firstFrameHappened;

    /// <inheritdoc cref="RendererMixer.renderTextureCache"/>
    readonly RenderTextureCache renderTextureCache = new RenderTextureCache();

    /// <inheritdoc cref="RendererMixer.texture2DCache"/>
    readonly Texture2DCache texture2DCache = new Texture2DCache();

    /// <inheritdoc cref="IMixer.RenderTextureCache"/>
    public RenderTextureCache RenderTextureCache => renderTextureCache;

    /// <inheritdoc cref="IMixer.Texture2DCache"/>
    public Texture2DCache Texture2DCache => texture2DCache;

    /// <inheritdoc cref="IMaterialProvider.Materials"/>
    public IEnumerable<Material> Materials => boundMaterial ?
        new Material[] { boundMaterial } : new Material[0];

    public override void OnPlayableDestroy(Playable playable)
    {
        firstFrameHappened = false;
        ResetMaterial();
    }

    void ResetMaterial()
    {
        // Restore original values
        if (boundMaterial && defaultMaterial)
            boundMaterial.CopyPropertiesFromMaterial(defaultMaterial);
    }

    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        boundMaterial = playerData as Material;
        if (boundMaterial == null)
            return;

        int inputCount = playable.GetInputCount();
        if (inputCount == 0)
            return;

        if (MaterialLayerMixer.frameClean)
        {
            // this mixer is mixing the first track layer
            MaterialLayerMixer.frameClean = false;

            if (firstFrameHappened)
            {
                // Reset bound material
                boundMaterial.CopyPropertiesFromMaterial(defaultMaterial);
            }
            else
            {
#if UNITY_EDITOR
                // Prevent Unity from saving the previewed version of
                // the bound material. Couldn't make it work via
                // TrackAsset.GatherProperties().
                UnityEditor.EditorApplication.quitting += ResetMaterial;
#endif

                // Save original value
                defaultMaterial = new Material(boundMaterial);
                firstFrameHappened = true;
            }
        }

        // Get clips contributing to the current frame (weight > 0)
        List<int> activeClips = Enumerable
            .Range(0, inputCount)
            .Where(i => playable.GetInputWeight(i) > 0)
            .ToList();

        if (activeClips.Count == 0)
            return;

        // The index of the first found active clip of this frame
        int clipIndex = activeClips[0];

        // Data stored in the first active clip
        var clipData = GetBehaviour(playable, clipIndex);

        // Weight of the first active clip
        float clipWeight = playable.GetInputWeight(clipIndex) * clipData.weightMultiplier;

        // The mixed property value to be applied to the bound material
        var mix = new MaterialBehaviour(clipData);

        if (activeClips.Count > 1)
        {
            // Two clips are blended.
            // As long as a valid LayerMixer exists, there can be at most two
            // active clips at one specific frame
            var next = GetBehaviour(playable, activeClips[1]);

            if (clipData.IsBlendableWith(next))
            {
                // Properties of blended clips match.
                // Mix current clip with next clip.
                mix.Lerp(next, clipData, clipWeight);
            }
            else
            {
                // Properties of blended clips don't match.
                // Individually mix them them with bound material

                // Next clip
                var mix2 = new MaterialBehaviour(next);
                mix2.ApplyFromMaterial(boundMaterial);
                mix2.Lerp(next, mix2, clipWeight);
                mix2.ApplyToMaterial(boundMaterial);

                // Current clip
                mix.ApplyFromMaterial(boundMaterial);
                mix.Lerp(mix, clipData, clipWeight);
            }
        }
        else if (clipWeight < 1)
        {
            // The clip blends with the layer background.
            // Mix clip with default material.
            mix.ApplyFromMaterial(boundMaterial);
            mix.Lerp(mix, clipData, clipWeight);
        }

        mix.ApplyToMaterial(boundMaterial);
    }

    /// <summary>
    /// Get behaviour at given port from given playable
    /// </summary>
    static MaterialBehaviour GetBehaviour(Playable playable, int inputPort)
        => ((ScriptPlayable<MaterialBehaviour>)playable.GetInput(inputPort))
           .GetBehaviour();
}
}
