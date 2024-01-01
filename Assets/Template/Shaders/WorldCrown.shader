Shader "DancingLine/WorldCrown"
{
    Properties
    {
        _MainTex ("Outline Texture", 2D) = "white" {}
        _FillTex ("Fill Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _FillColor ("Fill Color", Color) = (1, 1, 1, 1)
        _Progress ("Progress", Range(0, 1)) = 0
        // _Val ("Val", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { 
            "Queue" = "Transparent"  
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="true"  
        }

        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

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

            uniform sampler2D _FillTex;
            uniform float4 _OutlineColor;
            uniform float4 _FillColor;
            uniform float _Progress;
            uniform float _Val;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 fillCol = tex2D(_FillTex, i.uv) * _FillColor;
                fixed4 outlineCol = tex2D(_MainTex, i.uv) * _OutlineColor;

                fillCol.a *= _Progress;
                int inFill = outlineCol.a > 0.;

                fixed4 col = lerp(fillCol, outlineCol, inFill);
                
                // make fog work
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}