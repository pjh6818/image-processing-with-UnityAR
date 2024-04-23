Shader "Unlit/ShineShader"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _MaskTex ("MaskTex", 2D) = "white" {}
        _ShineColor ("ShineColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        // ZWrite Off
        //AlphaTest Greater 0.5

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _MaskTex;            
            fixed4 _ShineColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = v.uv;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, i.uv);

                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                col.g = col.r;
                col.b = col.r;
                col.a = col.a*mask.r;
                
                return col * _ShineColor;
            }
            ENDCG
        }
    }
}
