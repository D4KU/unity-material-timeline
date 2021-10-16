using UnityEngine.Playables;

public class MaterialLayerMixer : MaterialMixer
{
    public override void ProcessFrame(
        Playable playable,
        FrameData info,
        object playerData)
    {
        // The layer mixer is executed after all track layer mixers.
        // All that is left to do is to tell the first mixer of the next
        // frame that it's the first one
        MaterialMixer.frameClean = true;
    }
}
