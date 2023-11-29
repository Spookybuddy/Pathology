Shader "Unlit/Transition"
{
    Properties
    {
        _Scale("Scale", Float) = 1
        _MainTexture("Main Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
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

            sampler2D _MainTexture;
            float _Scale;
            float _Offset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float2 uvs = v.uv;
                uvs *= _Scale;
                _Offset = (_Scale - 1) / 2;
                uvs.x -= _Offset;
                uvs.y -= _Offset;
                o.uv = uvs;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 textureColor = tex2D(_MainTexture, i.uv);
                return textureColor;
            }
            ENDCG
        }
    }
}
