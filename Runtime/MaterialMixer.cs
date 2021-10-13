using UnityEngine;
using UnityEngine.Playables;
using System.Linq;

public class MaterialMixer<T, U> : PlayableBehaviour where T : MaterialBehaviour<U>, new()
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
            T data = GetBehaviour(playable, i);

            if (activeClips.Count() == 1)
            {
                if (weight < 1f)
                {
                    // Mix with default Material
                    MixWithDefault(data, weight);
                }
                else
                {
                    data.ToMaterial(data.value, boundMaterial);
                }
            }
            else
            {
                T next = GetBehaviour(playable, i + 1);
                if (data.GetType() == next.GetType() &&
                    data.propertyName == next.propertyName)
                {
                    // Mix with next clip
                    U newValue = data.Lerp(next.value, data.value, weight);
                    data.ToMaterial(newValue, boundMaterial);
                }
                else
                {
                    MixWithDefault(data, weight);
                    MixWithDefault(next, weight);
                }
            }

            return;
        }
    }

    static T GetBehaviour(Playable playable, int inputPort)
        => ((ScriptPlayable<T>)playable.GetInput(inputPort)).GetBehaviour();

    void MixWithDefault(T behaviour, float weight)
    {
        U defaultValue = behaviour.FromMaterial(defaultMaterial);
        U newValue = behaviour.Lerp(defaultValue, behaviour.value, weight);
        behaviour.ToMaterial(newValue, boundMaterial);
    }
}

public class IntMaterialMixer : MaterialMixer<IntMaterialBehaviour, int> {}
