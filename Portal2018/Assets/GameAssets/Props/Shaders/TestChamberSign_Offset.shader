﻿Shader "Vita/Portal/TestChamberSign_Offset"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,1)
		_Blend("Blend", Color) = (1,1,1,1)
		_UVCut("UV Cutoff", Range(0.0, 1.0)) = 1.0

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Offset -1, 1
		Pass
		{
			//Tags{ "LIGHTMODE" = "VertexLM" "RenderType" = "Opaque" }
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
			fixed4 _Tint;
			fixed4 _Blend;
			fixed _UVCut;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				clip(i.uv.x - (1.0-_UVCut));
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _Tint;
				col.rgb = col.rgb * _Blend.a + _Blend.rgb * (1.0 - _Blend.a);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
