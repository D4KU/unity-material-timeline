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

        // foreach (int i in activeClips)
        // {
        //     float clipWeight = playable.GetInputWeight(i);
        //     var input = (ScriptPlayable<MaterialBehaviour>)
        //         playable.GetInput(i);
        //     MaterialBehaviour clipData = input.GetBehaviour();
        //     MaterialBehaviour toApply = new MaterialBehaviour(clipData);

        //     if (activeClips.Count() > 1)
        //     {
        //         // Mix with next clip
        //         var nextInput = (ScriptPlayable<MaterialBehaviour>)
        //             playable.GetInput(i + 1);
        //         toApply.Lerp(nextInput.GetBehaviour(), clipData, clipWeight);
        //     }
        //     else
        //     {
        //         if (clipWeight < 1f)
        //         {
        //             // Mix with default Material
        //             toApply.ApplyFromMaterial(defaultMaterial);
        //             toApply.Lerp(toApply, clipData, clipWeight);
        //         }
        //     }

        //     toApply.ApplyToMaterial(boundMaterial);
        //     return;
        // }
    }
}
