using UnityEngine;
using UnityEngine.Playables;
using System.Linq;

public class MaterialMixer : PlayableBehaviour
{
    /// <summary>
    /// Material manipulated by the track
    /// </summary>
    private Material boundMaterial;

    /// <summary>
    /// Initial state before timeline executed
    /// </summary>
    private Material defaultMaterial;

    /// <summary>
    /// Initialization helper
    /// </summary>
    private bool firstFrameHappened;

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

        if (!firstFrameHappened)
        {
            // Save original value
            defaultMaterial = new Material(boundMaterial);
            firstFrameHappened = true;
        }

        int inputCount = playable.GetInputCount();
        if (inputCount == 0)
            return;

        boundMaterial.CopyPropertiesFromMaterial(defaultMaterial);

        // Get clips contributing to the current frame (weight > 0)
        var activeClips = from i in Enumerable.Range(0, inputCount)
                          where playable.GetInputWeight(i) > 0f
                          select i;

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
                    mix.ApplyFromMaterial(defaultMaterial);
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
                    // Individually mix them them with default material and
                    // apply them to bound material.

                    // Next clip
                    var mix2 = new MaterialBehaviour(next);
                    mix2.ApplyFromMaterial(defaultMaterial);
                    mix2.Lerp(next, mix2, weight);
                    mix2.ApplyToMaterial(boundMaterial);

                    // Current clip
                    mix.ApplyFromMaterial(defaultMaterial);
                    mix.Lerp(mix, data, weight);
                }
            }

            mix.ApplyToMaterial(boundMaterial);
            return;
        }
    }

    static MaterialBehaviour GetBehaviour(Playable playable, int inputPort)
    {
        var input = (ScriptPlayable<MaterialBehaviour>)playable.GetInput(inputPort);
        return input.GetBehaviour();
    }
}
