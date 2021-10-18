using UnityEngine;
using UnityEngine.Playables;
using System.Linq;

[System.Serializable]
public class RendererMixer : PlayableBehaviour
{
    const int DEFAULT_MATERIAL_INDEX = -1;
    static int oldMaterialIndex = DEFAULT_MATERIAL_INDEX;
    public int materialIndex = DEFAULT_MATERIAL_INDEX;

    /// <summary>
    /// Renderer manipulated by the track
    /// </summary>
    static Renderer boundRenderer;

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

        int materialCount = boundRenderer.sharedMaterials.Length;
        if (materialCount == 0)
            return;

        bool firstMixer = MaterialLayerMixer.frameClean;
        if (firstMixer)
        {
            // this mixer is mixing the first track layer
            MaterialLayerMixer.frameClean = false;
            ClearSlots();
        }

        // Get clips contributing to the current frame (weight > 0)
        int[] activeClips = (from i in Enumerable.Range(0, inputCount)
                             where playable.GetInputWeight(i) > 0f
                             select i).ToArray();
        if (activeClips.Length == 0)
            return;

        int clipIndex = activeClips[0];
        float weight = playable.GetInputWeight(clipIndex);
        MaterialBehaviour data = GetBehaviour(playable, clipIndex);
        var blocks = new MaterialPropertyBlock[materialCount];
        int start = 0;
        int end = materialCount;

        if (IsMaterialIndexValid(materialIndex))
        {
            start = materialIndex;
            end   = materialIndex + 1;
        }

        for (int slotIndex = start; slotIndex < end; slotIndex++)
        {
            var mix = new MaterialBehaviour(data);
            var block = new MaterialPropertyBlock();
            if (!firstMixer)
                boundRenderer.GetPropertyBlock(block, slotIndex);

            if (activeClips.Length == 1)
            {
                if (weight < 1f)
                {
                    // Mix clip into block
                    ApplyToBehaviour(mix, block, slotIndex, firstMixer);
                    mix.Lerp(mix, data, weight);
                }
            }
            else
            {
                // Two clips are blended.
                // As long as a valid LayerMixer exists, there can be at most
                // two active clips at one specific frame.
                var next = GetBehaviour(playable, clipIndex + 1);

                if (data.propertyType == next.propertyType &&
                    data.propertyName == next.propertyName)
                {
                    // Properties of blended clips match.
                    // Mix current clip with next clip.
                    mix.Lerp(next, data, weight);
                }
                else
                {
                    // Properties of blended clips don't match.
                    // Individually mix them them with property block

                    // Next clip
                    var mix2 = new MaterialBehaviour(next);
                    ApplyToBehaviour(mix2, block, slotIndex, firstMixer);
                    mix2.Lerp(next, mix2, weight);
                    mix2.ApplyToPropertyBlock(block);

                    // Current clip
                    ApplyToBehaviour(mix, block, slotIndex, firstMixer);
                    mix.Lerp(mix, data, weight);
                }
            }

            mix.ApplyToPropertyBlock(block);
            boundRenderer.SetPropertyBlock(block, slotIndex);
        }
    }

    static MaterialBehaviour GetBehaviour(Playable playable, int inputPort)
        => ((ScriptPlayable<MaterialBehaviour>)playable.GetInput(inputPort))
           .GetBehaviour();

    bool IsMaterialIndexValid(int index)
        => boundRenderer != null &&
           index >= 0 &&
           index < boundRenderer.sharedMaterials.Length;

    void ApplyToBehaviour(
        MaterialBehaviour mix,
        MaterialPropertyBlock block,
        int materialIndex,
        bool firstMixer)
    {
        if (firstMixer)
        {
            Material material = boundRenderer.sharedMaterials[materialIndex];
            if (material != null)
                mix.ApplyFromMaterial(material);
        }
        else
        {
            mix.ApplyFromPropertyBlock(block);
        }
    }

    void ClearSlots()
    {
        if (oldMaterialIndex == materialIndex)
        {
            if (IsMaterialIndexValid(materialIndex))
                boundRenderer.SetPropertyBlock(null, materialIndex);
            else
                ClearAllSlots();
        }
        else
        {
            if (IsMaterialIndexValid(materialIndex) &&
                IsMaterialIndexValid(oldMaterialIndex))
            {
                boundRenderer.SetPropertyBlock(null, materialIndex);
                boundRenderer.SetPropertyBlock(null, oldMaterialIndex);
            }
            else
            {
                ClearAllSlots();
            }

            oldMaterialIndex = materialIndex;
        }
    }

    void ClearAllSlots()
    {
        int materialCount = boundRenderer.sharedMaterials.Length;
        for (int i = 0; i < materialCount; i++)
            boundRenderer.SetPropertyBlock(null, i);
    }
}
