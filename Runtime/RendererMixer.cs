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

        var block = new MaterialPropertyBlock();
        bool firstMixer = MaterialLayerMixer.frameClean;

        if (firstMixer)
        {
            // this mixer is mixing the first track layer
            MaterialLayerMixer.frameClean = false;

            if (oldMaterialIndex != materialIndex)
            {
                SetPropertyBlock(null, oldMaterialIndex);
                oldMaterialIndex = materialIndex;
            }

            // Reset renderer
            SetPropertyBlock(null, materialIndex);
        }
        else
        {
            GetPropertyBlock(block);
        }

        // Get clips contributing to the current frame (weight > 0)
        var activeClips = from i in Enumerable.Range(0, inputCount)
                          where playable.GetInputWeight(i) > 0f
                          select i;

        // As long as a valid LayerMixer exists, there can be at most two
        // active clips at one specific frame
        foreach (int i in activeClips)
        {
            float weight = playable.GetInputWeight(i);
            var data = GetBehaviour(playable, i);
            var mix = new MaterialBehaviour(data);

            if (activeClips.Count() == 1)
            {
                if (weight < 1f)
                {
                    // Mix clip into block
                    ApplyToBehaviour(mix, block, firstMixer);
                    mix.Lerp(mix, data, weight);
                }
            }
            else
            {
                // Two clips are blended
                var next = GetBehaviour(playable, i + 1);

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
                    ApplyToBehaviour(mix2, block, firstMixer);
                    mix2.Lerp(next, mix2, weight);
                    mix2.ApplyToPropertyBlock(block);

                    // Current clip
                    ApplyToBehaviour(mix, block, firstMixer);
                    mix.Lerp(mix, data, weight);
                }
            }

            mix.ApplyToPropertyBlock(block);
            break;
        }

        SetPropertyBlock(block, materialIndex);
    }

    static MaterialBehaviour GetBehaviour(Playable playable, int inputPort)
        => ((ScriptPlayable<MaterialBehaviour>)playable.GetInput(inputPort))
           .GetBehaviour();

    bool IsMaterialIndexValid(int index)
        => boundRenderer != null &&
           index >= 0 &&
           index < boundRenderer.sharedMaterials.Length;

    void GetPropertyBlock(MaterialPropertyBlock block)
    {
        if (boundRenderer == null)
            return;

        if (IsMaterialIndexValid(materialIndex))
            boundRenderer.GetPropertyBlock(block, materialIndex);
        else
            boundRenderer.GetPropertyBlock(block);
    }

    void SetPropertyBlock(MaterialPropertyBlock block, int materialIndex)
    {
        if (boundRenderer == null)
            return;

        if (IsMaterialIndexValid(materialIndex))
            boundRenderer.SetPropertyBlock(block, materialIndex);
        else
            boundRenderer.SetPropertyBlock(block);
    }

    void ApplyToBehaviour(
        MaterialBehaviour mix,
        MaterialPropertyBlock block,
        bool firstMixer)
    {
        Material[] materials = boundRenderer.sharedMaterials;
        Material boundMaterial = null;

        if (IsMaterialIndexValid(materialIndex))
            boundMaterial = materials[materialIndex];

        if (firstMixer && boundMaterial != null)
            mix.ApplyFromMaterial(boundMaterial);
        else
            mix.ApplyFromPropertyBlock(block);
    }
}
