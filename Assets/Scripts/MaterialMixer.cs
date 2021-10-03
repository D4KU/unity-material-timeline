using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

namespace CustomTimeline
{
    using PropertyType = MaterialBehaviour.PropertyType;

    public class MaterialMixer : PlayableBehaviour
    {
        private Material bindMaterial;
        private Material presetMaterial;
        private bool firstFrameHappened;

        public override void OnPlayableDestroy(Playable playable)
        {
            firstFrameHappened = false;

            if (bindMaterial == null)
                return;

            // Restore original values
            if (presetMaterial != null)
                bindMaterial.CopyPropertiesFromMaterial(presetMaterial);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            bindMaterial = playerData as Material;
            if (bindMaterial == null)
                return;

            if (!firstFrameHappened)
            {
                presetMaterial = new Material(bindMaterial);
                firstFrameHappened = true;
            }

            int inputCount = playable.GetInputCount();
            if (inputCount == 0)
                return;

            var activeClips = from i in Enumerable.Range(0, inputCount)
                              where playable.GetInputWeight(i) > 0f
                              select i;

            foreach (int i in activeClips)
            {
                float clipWeight = playable.GetInputWeight(i);
                var input = (ScriptPlayable<MaterialBehaviour>)
                    playable.GetInput(i);
                MaterialBehaviour clipData = input.GetBehaviour();
                MaterialBehaviour toApply = clipData;

                if (activeClips.Count() > 1)
                {
                    // Mix with next clip
                    toApply = new MaterialBehaviour(clipData);
                    var nextInput = (ScriptPlayable<MaterialBehaviour>)
                        playable.GetInput(i + 1);
                    toApply.Lerp(
                            nextInput.GetBehaviour(),
                            clipData,
                            clipWeight);
                }
                else
                {
                    toApply = new MaterialBehaviour(clipData);
                    if (clipWeight < 1f)
                    {
                        // Mix with preset Material
                        toApply.ApplyFromMaterial(presetMaterial);
                        toApply.Lerp(toApply, clipData, clipWeight);
                    }
                }

                toApply.ApplyToMaterial(bindMaterial);
                return;
            }

            // No clip was found with weight > 0
            bindMaterial.CopyPropertiesFromMaterial(presetMaterial);
        }
    }
}
