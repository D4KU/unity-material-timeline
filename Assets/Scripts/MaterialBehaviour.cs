using UnityEngine.Playables;
using UnityEngine;
using System;

namespace CustomTimeline
{
    // TODO Only store the values used.
    // Create a clip for each property type?
    [Serializable]
    public class MaterialBehaviour : PlayableBehaviour
    {
        public enum PropertyType
        {
            Int,
            Float,
            Texture,
            TextureTiling,
            TextureOffset,
            Color,
            Vector
        }

        public string propertyName;
        public PropertyType propertyType;
        public int intValue;
        public float floatValue;
        public Texture texture;
        public Vector2 tiling;
        public Vector2 offset;
        public Color color;
        public Vector4 vector;

        static public MaterialBehaviour Lerp(
            MaterialBehaviour a,
            MaterialBehaviour b,
            float t)
        {
            var c = new MaterialBehaviour();
            c.propertyName = a.propertyName;
            c.propertyType = a.propertyType;

            switch (a.propertyType)
            {
                case PropertyType.Int:
                    c.intValue = (int)Mathf.Lerp(a.intValue, b.intValue, t);
                    break;
                case PropertyType.Float:
                    c.floatValue = Mathf.Lerp(a.floatValue, b.floatValue, t);
                    break;
                case PropertyType.Texture:
                    c.texture = a.texture;
                    break;
                case PropertyType.Color:
                    c.color = Color.Lerp(a.color, b.color, t);
                    break;
                case PropertyType.TextureTiling:
                    c.tiling = Vector2.Lerp(a.tiling, b.tiling, t);
                    break;
                case PropertyType.TextureOffset:
                    c.offset = Vector2.Lerp(a.offset, b.offset, t);
                    break;
                case PropertyType.Vector:
                    c.vector = Vector4.Lerp(a.vector, b.vector, t);
                    break;
            }
            return c;
        }
    }
}
