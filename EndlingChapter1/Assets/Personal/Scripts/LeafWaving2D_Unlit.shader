Shader "Custom/LeafWaving2D_Unlit"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color   ("Tint", Color)        = (1,1,1,1)

        _WindStrength  ("Wind Strength", Range(0, 0.5)) = 0.1
        _WindFrequency ("Wind Frequency", Range(0, 10)) = 2.0
        _WindScale     ("Wind Scale",    Range(0, 10)) = 3.0
        _WindDirection ("Wind Direction (XY)", Vector) = (1, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Color;

            float _WindStrength;
            float _WindFrequency;
            float _WindScale;
            float4 _WindDirection;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float2 dir = normalize(_WindDirection.xy + float2(1e-5, 0));
                float mask = saturate(v.uv.y);

                float phase  = v.uv.x * _WindScale + _Time.y * _WindFrequency;
                float offset = sin(phase) * _WindStrength * mask;

                v.vertex.xy += dir * offset;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv) * _Color;
            }

            ENDCG
        }
    }
}
