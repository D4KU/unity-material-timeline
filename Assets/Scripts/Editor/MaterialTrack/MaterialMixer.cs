using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor.Presets;

namespace CustomTimeline
{
    public class MaterialMixer : PlayableBehaviour
    {

        internal PlayableDirector director;

        // what's IEnumerable -> It's Array
        internal IEnumerable<TimelineClip> clips;

        public Material bindMaterial;

        private Preset presetMaterial;

        public override void OnGraphStart(Playable playable)
        {
            if (bindMaterial == null)
                return;
            //プレビュー前に値を保持しておく
            if (presetMaterial == null)
                presetMaterial = new Preset(bindMaterial);
            else
                presetMaterial.UpdateProperties(bindMaterial);
        }

        public override void OnGraphStop(Playable playable)
        {
            if (bindMaterial == null)
                return;
            //プレビュー終了後に保持した値をコピーして最初に戻す
            if(presetMaterial != null)
                presetMaterial.ApplyTo(bindMaterial);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            int inputCount = playable.GetInputCount();
            if (inputCount == 0)
                return;
            if (bindMaterial == null)
                return;

            MaterialProperty finalProperty = new MaterialProperty();

            double progressTime = director.time;
            IEnumerator<TimelineClip> enumerator = clips.GetEnumerator();
            enumerator.MoveNext();
            for (int i = 0; i < inputCount; i++)
            {
                TimelineClip clip = enumerator.Current;
                MaterialPlayableAsset asset = clip.asset as MaterialPlayableAsset;
                float clipWeight = playable.GetInputWeight(i);
                if (1.0f > clipWeight && clipWeight > 0.0f)
                {
                    enumerator.MoveNext();
                    TimelineClip nextClip = enumerator.Current;
                    MaterialPlayableAsset nextAsset = nextClip.asset as MaterialPlayableAsset;

                    SwitchUpdateProperty(ref finalProperty, asset, nextAsset, 1.0f - clipWeight);

                    break;
                }

                if (progressTime >= clip.start && progressTime <= clip.end)
                {
                    finalProperty = asset.property;
                    break;
                }


                enumerator.MoveNext();
            }

            SwitchUpdateMaterial(finalProperty);

        }

        public void SwitchUpdateProperty(ref MaterialProperty property, MaterialPlayableAsset asset, MaterialPlayableAsset nextAsset, float weight)
        {
            property.propertyName = asset.property.propertyName;
            property.propertyType = asset.property.propertyType;

            switch (property.propertyType)
            {
                case PropertyType.Int:
                    property.intValue = (int)Mathf.Lerp(asset.property.intValue, nextAsset.property.intValue, weight);
                    break;
                case PropertyType.Float:
                    property.floatValue = Mathf.Lerp(asset.property.floatValue, nextAsset.property.floatValue, weight);
                    break;
                case PropertyType.Color:
                    property.color = Color.Lerp(asset.property.color, nextAsset.property.color, weight);
                    break;
                case PropertyType.Texture:
                    property.texture = asset.property.texture;
                    break;
                case PropertyType.TextureTiling:
                    property.tilling = Vector2.Lerp(asset.property.tilling, nextAsset.property.tilling, weight);
                    break;
                case PropertyType.TextureOffset:
                    property.offset = Vector2.Lerp(asset.property.offset, nextAsset.property.offset, weight);
                    break;
                case PropertyType.Vector:
                    property.vector = Vector4.Lerp(asset.property.vector, nextAsset.property.vector, weight);
                    break;
                default:
                    break;
            }
        }

        public void SwitchUpdateMaterial(MaterialProperty property)
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
                    bindMaterial.SetTextureScale(property.propertyName, property.tilling);
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
