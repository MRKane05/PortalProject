Shader "Vita/Portal/TestChamberSign"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,1)
		_Blend("Blend", Color) = (1,1,1,1)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100
			Pass
			{
				//Tags{ "LIGHTMODE" = "VertexLM" "RenderType" = "Opaque" }
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			// Add lightmap support
			#pragma multi_compile_fwdbase
			#pragma multi_compile _ LIGHTMAP_ON

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1; // lightmap UVs
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				#ifdef LIGHTMAP_ON
				float2 lmap : TEXCOORD1; // lightmap UVs
				#endif
				UNITY_FOG_COORDS(2)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Tint;
			fixed4 _Blend;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				#ifdef LIGHTMAP_ON
				o.lmap = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _Tint;
				col.rgb = col.rgb * _Blend.a + _Blend.rgb * (1.0 - _Blend.a);
				/*
				// Apply lightmap
				#ifdef LIGHTMAP_ON
				fixed3 lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));
				col.rgb *= lightmap;
				#endif*/

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}