Shader "Unlit/CheckerboardShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            //Tags { "LightMode" = "ExampleLightModeTag"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata  
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                //float4 vertex : POSITION;
            };

            struct fragOutput {
                fixed4 color : COLOR;
                float depth : DEPTH;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _Color2;

            v2f vert (appdata v, out float4 outpos : POSITION )
            {
                v2f o;
                outpos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,outpos);
                return o;
            }

            fragOutput frag (v2f i, UNITY_VPOS_TYPE screenPos : VPOS)
            {
                fragOutput o;
                
                
                screenPos.xy = floor(screenPos.xy * 0.5) * 0.5;
                float checker = -frac(screenPos.r + screenPos.g);

                if (checker < 0)
                {
                    // sample the texture
                    o.color = tex2D(_MainTex, i.uv) * _Color;
                    o.depth = 1;
                }
                else
                {
                    o.color = tex2D(_MainTex, i.uv) * _Color2;
                    o.depth = 0;
                }

                // // clip HLSL instruction stops rendering a pixel if value is negative
                // clip(checker);


                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, color);
                return o;
            }
            ENDCG
        }
    }
    Fallback Off
}
