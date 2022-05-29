using UnityEngine;
using UnityEngine.Playables;

namespace MaterialTrack
{
[System.Serializable]
public class SlotLayerMixer : PlayableBehaviour
{
    public static Material[] overrides;

    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        // Because the layer mixer is always executed after the track mixer of
        // each associated layer ran, we misuse it to store a shared
        // state between each track mixer, which is applied to the bound
        // renderer, reset, and then built anew each frame.
        if (playerData is Renderer renderer)
            renderer.sharedMaterials = overrides;
        overrides = null;
    }
}
}
