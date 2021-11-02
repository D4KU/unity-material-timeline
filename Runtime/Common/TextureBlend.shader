Shader "Hidden/MaterialTrackTexLerp"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SideTex ("Texture", 2D) = "white" {}
        _weight ("weight", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _SideTex;
            float4 _SideTex_ST;

            float _weight;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 a = tex2D(_MainTex, i.uv);
                fixed4 b = tex2D(_SideTex, i.uv);
                return (1 - _weight) * a + _weight * b;
            }
            ENDCG
        }
    }
}
