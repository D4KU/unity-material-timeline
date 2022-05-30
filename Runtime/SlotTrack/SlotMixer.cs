using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace MaterialTrack
{
[System.Serializable]
public class SlotMixer : PlayableBehaviour
{
    /// <summary>
    /// Renderer manipulated by the track
    /// </summary>
    [HideInInspector] public Renderer boundRenderer;

    /// <summary>
    /// Initial materials assigned to each slot of <see cref="boundRenderer"/>
    /// before the first frame was processed.
    /// </summary>
    [HideInInspector] public Material[] initialMaterials;

    /// <summary>
    /// Assumed to be of equal length as <see cref="initialMaterials"/>.
    /// Stores for each slot whether to override its initial material.
    /// </summary>
    public bool[] mask;

    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        if (initialMaterials == null)
            return;

        // Reinitialize shared state with other mixers if we are the first one
        // to run this frame
        SlotLayerMixer.overrides ??= initialMaterials.ToArray();

        // Find the currently played clip
        for (int clipIdx = 0; clipIdx < playable.GetInputCount(); clipIdx++)
        {
            if (playable.GetInputWeight(clipIdx) > .5f)
            {
                Material _override = GetBehaviour(playable, clipIdx).material;

                // Apply the current clip's material to each slot passing the
                // mask
                for (int slotIdx = 0; slotIdx < mask.Length; slotIdx++)
                    if (mask[slotIdx])
                        SlotLayerMixer.overrides[slotIdx] = _override;

                // No other clip can be active on this track, because blending
                // is deactivated.
                return;
            }
        }
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        if (boundRenderer != null)
            boundRenderer.sharedMaterials = initialMaterials;
    }

    /// <summary>
    /// Get behaviour at given port from given playable
    /// </summary>
    static SlotBehaviour GetBehaviour(Playable playable, int inputPort)
        => ((ScriptPlayable<SlotBehaviour>)playable.GetInput(inputPort))
           .GetBehaviour();
}
}
