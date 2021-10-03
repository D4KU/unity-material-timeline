using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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

            for (int i = 0; i < inputCount; i++)
            {
                float clipWeight = playable.GetInputWeight(i);
                if (clipWeight == 0f)
                    continue;

                MaterialBehaviour toApply;
                var input = (ScriptPlayable<MaterialBehaviour>)
                    playable.GetInput(i);

                if (clipWeight < 1f)
                {
                    if (i == inputCount - 1)
                    {
                        // Last clip
                        // TODO Blend with presetMaterial
                        toApply = input.GetBehaviour();
                    }
                    else
                    {
                        // Mix with next clip
                        var nextInput = (ScriptPlayable<MaterialBehaviour>)
                            playable.GetInput(i + 1);
                        toApply = MaterialBehaviour.Lerp(
                            nextInput.GetBehaviour(),
                            input.GetBehaviour(),
                            clipWeight);
                    }
                }
                else
                {
                    // Weight is 1, no mixing
                    toApply = input.GetBehaviour();
                }

                SwitchUpdateMaterial(toApply);
                break;
            }
        }

        public void SwitchUpdateMaterial(MaterialBehaviour property)
        {
            switch (property.propertyType)
            {
                case PropertyType.Int:
                    bindMaterial.SetInt(property.propertyName, property.intValue);
                    break;
                case PropertyType.Float:
                    bindMaterial.SetFloat(property.propertyName, property.floatValue);
                    break;
                case PropertyType.Color:
                    bindMaterial.SetColor(property.propertyName, property.color);
                    break;
                case PropertyType.Texture:
                    bindMaterial.SetTexture(property.propertyName, property.texture);
                    break;
                case PropertyType.TextureTiling:
                    bindMaterial.SetTextureScale(property.propertyName, property.tiling);
                    break;
                case PropertyType.TextureOffset:
                    bindMaterial.SetTextureOffset(property.propertyName, property.offset);
                    break;
                case PropertyType.Vector:
                    bindMaterial.SetVector(property.propertyName, property.vector);
                    break;
                default:
                    break;
            }
        }

    }
}
