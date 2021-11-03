using UnityEngine.Playables;

namespace MaterialTrack
{
public class MaterialLayerMixer : PlayableBehaviour
{
    /// <summary>
    /// True for the mixer of the first track layer
    /// </summary>
    public static bool frameClean = true;

    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        // The layer mixer is executed after all track layer mixers.
        // All that is left to do is to tell the first mixer of the next
        // frame that it's the first one
        MaterialLayerMixer.frameClean = true;
    }
}
}
