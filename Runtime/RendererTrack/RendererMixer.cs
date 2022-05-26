using UnityEngine;
using UnityEngine.Playables;
using System.Linq;
using System.Collections.Generic;

namespace MaterialTrack
{
[System.Serializable]
public class RendererMixer : PlayableBehaviour, IMaterialProvider
{
    /// Used for serialization in this class's inspector drawer
    public const string MAT_IDX_FIELD = nameof(materialIndex);

    const int DEFAULT_MATERIAL_INDEX = -1;

    [Tooltip("If non-negative, specifies one of the bound renderer's " +
        "materials to override exclusively. If negative, all materials " +
        "are overridden.")]
    public int materialIndex = DEFAULT_MATERIAL_INDEX;

    /// <summary>
    /// Helper to notice when user changes serialized material index.
    /// </summary>
    int oldMaterialIndex = DEFAULT_MATERIAL_INDEX;

    /// <summary>
    /// Renderer manipulated by the track
    /// </summary>
    Renderer boundRenderer;

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
    /// The number of materials the bound renderer references
    /// </summary>
    int MaterialCount => AvailableMaterials.Length;

    /// <summary>
    /// Materials operated on
    /// </summary>
    public IEnumerable<Material> Materials
    {
        get
        {
            if (boundRenderer == null)
                return new Material[0];

            if (IsMaterialIndexValid(materialIndex))
                return new Material[]
                {
                    AvailableMaterials[materialIndex]
                };

            return AvailableMaterials;
        }
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        if (boundRenderer != null)
            ResetAllSlots();
    }

    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        boundRenderer = playerData as Renderer;
        if (boundRenderer == null)
            return;

        int inputCount = playable.GetInputCount();
        if (inputCount == 0)
            return;

        int materialCount = MaterialCount;
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
                ResetTargetSlots();
        }

        // Get clips contributing to the current frame (weight > 0)
        int[] activeClips = (from i in Enumerable.Range(0, inputCount)
                             where playable.GetInputWeight(i) > 0f
                             select i).ToArray();
        if (activeClips.Length == 0)
            return;

        // The index of the first found active clip of this frame
        int clipIndex = activeClips[0];

        // Weight of the first active clip
        float clipWeight = playable.GetInputWeight(clipIndex);

        // Data stored in the first active clip
        RendererBehaviour clipData = GetBehaviour(playable, clipIndex);

        // Blocks that will be created during the process
        var blocks = new MaterialPropertyBlock[materialCount];

        // If the user-entered material slot index is valid, we only have
        // to operate on this one slot, otherwise on all.
        int startSlot = 0;
        int endSlot = materialCount;
        if (IsMaterialIndexValid(materialIndex))
        {
            startSlot = materialIndex;
            endSlot   = materialIndex + 1;
        }

        for (int slotIndex = startSlot; slotIndex < endSlot; slotIndex++)
        {
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
        if (source.isEmpty)
        {
            Material material = AvailableMaterials[fallbackMaterialIndex];
            if (material != null)
                target.ApplyFromMaterial(material);
        }
        else
            target.ApplyFromPropertyBlock(source);
    }

    /// <summary>
    /// Returns true if the given material index is available in the bound
    /// renderer.
    /// </summary>
    bool IsMaterialIndexValid(int index)
        => index >= 0 && index < MaterialCount;

    /// <summary>
    /// Stores <see cref="MaterialPropertyBlock"/>s present before the
    /// creation of the mixer so it can layer its properties on top.
    /// </summary>
    void CacheInitialBlocks()
    {
        int count = MaterialCount;
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
    /// Remove the material property block from all materials slots of
    /// the bound renderer that this mixer targets.
    /// </summary>
    void ResetTargetSlots()
    {
        // If the user has updated the material index since the last frame,
        // we need to clear the newly AND previously set slot.
        if (oldMaterialIndex == materialIndex)
        {
            // Material index didn't update
            // If the current index is out of bounds, consider it to
            // target all slots.
            if (IsMaterialIndexValid(materialIndex))
                boundRenderer.SetPropertyBlock(
                    initialBlocks[materialIndex],
                    materialIndex);
            else
                ResetAllSlots();
        }
        else
        {
            // Material index updated
            // If the index was or is out of bounds, consider it to
            // target all slots.
            if (IsMaterialIndexValid(materialIndex) &&
                IsMaterialIndexValid(oldMaterialIndex))
            {
                boundRenderer.SetPropertyBlock(
                    initialBlocks[materialIndex],
                    materialIndex);
                boundRenderer.SetPropertyBlock(
                    initialBlocks[oldMaterialIndex],
                    oldMaterialIndex);
            }
            else
                ResetAllSlots();

            oldMaterialIndex = materialIndex;
        }
    }

    /// <summary>
    /// Reassign property blocks present before creation of this mixer,
    /// delete all others.
    /// </summary>
    void ResetAllSlots()
    {
        int end = MaterialCount;
        for (int i = 0; i < end; i++)
            boundRenderer.SetPropertyBlock(initialBlocks?[i], i);
    }
}
}
