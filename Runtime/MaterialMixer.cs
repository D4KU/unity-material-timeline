using UnityEngine;
using UnityEngine.Playables;
using System.Linq;

public class MaterialMixer : PlayableBehaviour
{
    /// <summary>
    /// Material manipulated by the track
    /// </summary>
    static Material boundMaterial;

    /// <summary>
    /// Material state before timeline initialized
    /// </summary>
    static Material defaultMaterial;

    /// <summary>
    /// Initialization helper
    /// </summary>
    static bool firstFrameHappened;

    public override void OnPlayableDestroy(Playable playable)
    {
        firstFrameHappened = false;

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
            float weight = playable.GetInputWeight(i);
            var data = GetBehaviour(playable, i);
            var mix = new MaterialBehaviour(data);

            if (activeClips.Count() == 1)
            {
                if (weight < 1f)
                {
                    // Mix clip with default material
                    mix.ApplyFromMaterial(boundMaterial);
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

    static MaterialBehaviour GetBehaviour(Playable playable, int inputPort)
        => ((ScriptPlayable<MaterialBehaviour>)playable.GetInput(inputPort))
           .GetBehaviour();
}
