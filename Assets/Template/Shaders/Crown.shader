Shader "DancingLine/Crown"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FillTex ("Fill Texture", 2D) = "white" {}
        _FillColor ("Fill Color", Color) = (1, 1, 1, 1)
        _Enable ("Enable", Range(0, 1)) = 0
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
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;

            uniform sampler2D _MainTex;
            uniform sampler2D _FillTex;
            uniform fixed4 _FillColor;
            uniform fixed _Enable;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 fillColor = tex2D(_FillTex, IN.texcoord) * _FillColor;
                
                if (_Enable * 0.95 < IN.texcoord.x) fillColor = 0;
                else fillColor.a *= _Enable;
                
                const fixed4 outlineColor = tex2D(_MainTex, IN.texcoord);
                fixed4 color = saturate(fillColor + outlineColor);
                color.a *= IN.color.a;
                return color;
            }
        ENDCG
        }
    }
}