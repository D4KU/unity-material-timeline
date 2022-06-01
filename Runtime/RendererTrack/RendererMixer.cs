using UnityEngine;
using UnityEngine.Playables;
using System.Linq;
using System.Collections.Generic;

namespace MaterialTrack
{
[System.Serializable]
public class RendererMixer : PlayableBehaviour, IMaterialProvider
{
    /// <summary>
    /// Assumed to be of equal length as <see cref="AvailableMaterials"/>.
    /// Stores for each slot whether to apply a property block to it.
    /// </summary>
    public bool[] mask;

    /// <summary>
    /// Renderer manipulated by the track
    /// </summary>
    [HideInInspector] public Renderer boundRenderer;

    /// <summary>
    /// Stores blocks present before the creation of the mixer so it can layer
    /// its properties on top and reset them on destruction. One entry per
    /// available material.
    /// </summary>
    MaterialPropertyBlock[] initialBlocks;

    /// <summary>
    /// All materials the bound renderer references
    /// </summary>
    Material[] AvailableMaterials => boundRenderer.sharedMaterials;

    /// <summary>
    /// Materials operated on
    /// </summary>
    public IEnumerable<Material> Materials
    {
        get
        {
            if (boundRenderer == null)
                return new Material[0];
            return AvailableMaterials.Where((_, i) => mask[i]);
        }
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        if (boundRenderer != null)
            ResetSlots();
    }

    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        if (boundRenderer == null)
            return;

        int inputCount = playable.GetInputCount();
        if (inputCount == 0)
            return;

        int materialCount = AvailableMaterials.Length;
        if (materialCount == 0)
            return;

        // Is this mixer mixing the first track layer?
        if (MaterialLayerMixer.frameClean)
        {
            MaterialLayerMixer.frameClean = false;

            // True only in the very first call since creation of the mixer
            if (initialBlocks == null)
                CacheInitialBlocks();
            else
                ResetSlots();
        }

        // Get clips contributing to the current frame (weight > 0)
        int[] activeClips = (from i in Enumerable.Range(0, inputCount)
                             where playable.GetInputWeight(i) > 0f
                             select i).ToArray();
        if (activeClips.Length == 0)
            return;

        // The index of the first found active clip of this frame
        int clipIndex = activeClips[0];

        // Data stored in the first active clip
        RendererBehaviour clipData = GetBehaviour(playable, clipIndex);

        // Weight of the first active clip
        float clipWeight = playable.GetInputWeight(clipIndex) * clipData.weightMultiplier;

        // Blocks that will be created during the process
        var blocks = new MaterialPropertyBlock[materialCount];

        for (int slotIndex = 0; slotIndex < materialCount; slotIndex++)
        {
            if (!mask[slotIndex])
                continue;

            // The property block to apply to the current slot index
            var block = new MaterialPropertyBlock();

            // The mixed property value to apply to the property block at the
            // current slot index.
            var mix = new RendererBehaviour(clipData);

            // Use blocks already present as starting point and mix to it
            boundRenderer.GetPropertyBlock(block, slotIndex);

            // Only consider the per-Renderer block if there is no
            // slot-specific one. This mimics Unity's general behaviour:
            // the property set of a per-Renderer block isn't extended by an
            // index-specific block, it is hidden.
            if (block.isEmpty)
                boundRenderer.GetPropertyBlock(block);

            if (activeClips.Length == 1)
            {
                if (clipWeight < 1f)
                {
                    // The clip blends with the layer background.
                    // Mix clip into block.
                    ApplyToBehaviour(block, mix, slotIndex);
                    mix.Lerp(mix, clipData, clipWeight);
                }
            }
            else
            {
                // Two clips are blended.
                // As long as a valid LayerMixer exists, there can be at most
                // two active clips at one specific frame.
                var next = GetBehaviour(playable, clipIndex + 1);

                if (clipData.IsBlendableWith(next))
                {
                    // Properties of blended clips match.
                    // Mix current clip with next clip.
                    mix.Lerp(next, clipData, clipWeight);
                }
                else
                {
                    // Properties of blended clips don't match.
                    // Individually mix them them with property block

                    // Next clip
                    var mix2 = new RendererBehaviour(next);
                    ApplyToBehaviour(block, mix2, slotIndex);
                    mix2.Lerp(next, mix2, clipWeight);
                    mix2.ApplyToPropertyBlock(block);

                    // Current clip
                    ApplyToBehaviour(block, mix, slotIndex);
                    mix.Lerp(mix, clipData, clipWeight);
                }
            }

            mix.ApplyToPropertyBlock(block);
            boundRenderer.SetPropertyBlock(block, slotIndex);
        }
    }

    /// <summary>
    /// Get behaviour at given port from given playable
    /// </summary>
    static RendererBehaviour GetBehaviour(Playable playable, int inputPort)
        => ((ScriptPlayable<RendererBehaviour>)playable.GetInput(inputPort))
           .GetBehaviour();

    /// <summary>
    /// Set the shader property of <paramref name="target"/>, with the value
    /// taken from <paramref name="source"/> if it isn't empty, and from the
    /// bound material specified by <paramref name="fallbackMaterialIndex"/>
    /// otherwise.
    /// </summary>
    void ApplyToBehaviour(
        MaterialPropertyBlock source,
        RendererBehaviour target,
        int fallbackMaterialIndex)
    {
        if (target.ContainsPropertyOf(source))
            target.ApplyFromPropertyBlock(source);
        else
        {
            Material material = AvailableMaterials[fallbackMaterialIndex];
            if (material != null)
                target.ApplyFromMaterial(material);
        }
    }

    /// <summary>
    /// Stores <see cref="MaterialPropertyBlock"/>s present before the
    /// creation of the mixer so it can layer its properties on top.
    /// </summary>
    void CacheInitialBlocks()
    {
        int count = AvailableMaterials.Length;
        initialBlocks = new MaterialPropertyBlock[count];
        var block = new MaterialPropertyBlock();

        for (int i = 0; i < count; i++)
        {
            boundRenderer.GetPropertyBlock(block, i);
            if (block.isEmpty)
                continue;

            initialBlocks[i] = block;
            block = new MaterialPropertyBlock();
        }
    }

    /// <summary>
    /// Reassign property blocks present before creation of this mixer,
    /// delete all others.
    /// </summary>
    void ResetSlots()
    {
        // In theory we should only reset slots that are not masked out,
        // but that proved difficult when user just changed the mask.
        // I deemed this not worth the additional code complexity.
        int end = AvailableMaterials.Length;
        for (int i = 0; i < end; i++)
            boundRenderer.SetPropertyBlock(initialBlocks?[i], i);
    }
}
}
