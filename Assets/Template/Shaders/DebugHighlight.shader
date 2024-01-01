// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DancingLine/Debug/Highlight"
{
	Properties
	{
		_OutlineColor ("Outline Color", Color) = (1, .5, 0, 1)
		_OutlineScale ("Outline Scale", Range(0, 1)) = .1
		_InverseColor ("Inverse Color", Range(0, 1)) = 1
	}
    SubShader
    {
        Tags { 
            "Queue" = "Transparent+1"  
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="true"  
        }
        
		ZTest Less
        
		GrabPass { "_GrabTex" }
		Pass {
		CGPROGRAM
			#include "UnityCG.cginc"
			#include "ColorSpace.cginc"
			
			#pragma vertex vert
			#pragma fragment frag
				
			sampler2D _GrabTex;

			struct input {
				fixed4 vertex : POSITION;
				fixed2 texcoord : TEXCOORD0;
			};	
			struct v2f {
				fixed4 vertex : POSITION;
				fixed2 texcoord : TEXCOORD0;
				fixed4 uvgrab : TEXCOORD1;
			};

			uniform fixed _InverseColor;
			
			v2f vert (input v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.uvgrab = ComputeGrabScreenPos(o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : COLOR {
				const fixed2 uv = i.uvgrab.xy / i.uvgrab.w;
				const fixed4 col = tex2D(_GrabTex, uv);
				float3 colHsv = rgb2hsv(1 - col);
				colHsv.r += .5;
				return saturate(lerp(col, fixed4(hsv2rgb(colHsv), col.a), _InverseColor));
			}
		ENDCG
	    }


		Pass
		{
		Cull Front
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			struct input {
				fixed4 vertex : POSITION;
				fixed2 texcoord : TEXCOORD0;
				fixed4 normal : NORMAL;
			};	
			struct v2f {
				fixed4 vertex : POSITION;
			};

			uniform fixed4 _OutlineColor;
			uniform fixed _OutlineScale;
			
			v2f vert (input v) {
				v2f o;
				v.vertex *= 1 + _OutlineScale;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag (v2f i) : COLOR {
				return _OutlineColor;
			}
		ENDCG
		}
    }
}