using UnityEngine;
using UnityEngine.Playables;
using System.Linq;
using System.Collections.Generic;

namespace MaterialTrack
{
public class MaterialMixer : PlayableBehaviour, IMaterialProvider
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

    public IEnumerable<Material> Materials => boundMaterial == null ?
        new Material[0] : new Material[] { boundMaterial };

    public override void OnPlayableDestroy(Playable playable)
    {
        firstFrameHappened = false;
        ResetMaterial();
    }

    void ResetMaterial()
    {
        // Restore original values
        if (boundMaterial != null && defaultMaterial != null)
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
        var activeClips = from i in Enumerable.Range(0, inputCount)
                          where playable.GetInputWeight(i) > 0f
                          select i;

        // As long as a valid LayerMixer exists, there can be at most two
        // active clips at one specific frame
        foreach (int i in activeClips)
        {
            // Weight of the first active clip
            float weight = playable.GetInputWeight(i);

            // Data stored in the first active clip
            var data = GetBehaviour(playable, i);

            // The mixed property value to be applied to the bound material
            var mix = new MaterialBehaviour(data);

            if (activeClips.Count() == 1)
            {
                if (weight < 1f)
                {
                    // The clip blends with the layer background.
                    // Mix clip with default material.
                    mix.ApplyFromMaterial(boundMaterial);
                    mix.Lerp(mix, data, weight);
                }
            }
            else
            {
                // Two clips are blended
                var next = GetBehaviour(playable, i + 1);

                if (data.IsBlendableWith(next))
                {
                    // Properties of blended clips match.
                    // Mix current clip with next clip.
                    mix.Lerp(next, data, weight);
                }
                else
                {
                    // Properties of blended clips don't match.
                    // Individually mix them them with bound material

                    // Next clip
                    var mix2 = new MaterialBehaviour(next);
                    mix2.ApplyFromMaterial(boundMaterial);
                    mix2.Lerp(next, mix2, weight);
                    mix2.ApplyToMaterial(boundMaterial);

                    // Current clip
                    mix.ApplyFromMaterial(boundMaterial);
                    mix.Lerp(mix, data, weight);
                }
            }

            mix.ApplyToMaterial(boundMaterial);
            return;
        }
    }

    /// <summary>
    /// Get behaviour at given port from given playable
    /// </summary>
    static MaterialBehaviour GetBehaviour(Playable playable, int inputPort)
        => ((ScriptPlayable<MaterialBehaviour>)playable.GetInput(inputPort))
           .GetBehaviour();
}
}
