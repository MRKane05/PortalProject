Shader "Mobile/Effects/VirtualRoom_Lite"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Parallax("Parallax", float) = 1.0
		[NoScaleOffset] _BumpMap("Normalmap", 2D) = "bump" {}
		_BumpDistort("BumpDistort", float) = 1.0
	}
	SubShader
	{
		Tags{ "LIGHTMODE" = "VertexLM" "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent: TANGENT;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _BumpMap;
			float4 _MainTex_ST;
			float _Parallax;
			float _BumpDistort;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = o.uv;

				//Pull back our UV
				o.uv = lerp(o.uv, float2(0.5f, 0.5f), 0.5f);

				//Simple paralax system
				float3 binormal = cross(v.normal, v.tangent.xyz)
					* v.tangent.w;
				// appropriately scaled tangent and binormal 
				// to map distances from object space to texture space
				float3 viewDirInObjectCoords = mul(
					unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz
					- v.vertex.xyz;
				float3x3 localSurface2ScaledObjectT =
					float3x3(v.tangent.xyz, binormal, v.normal);
				// vectors are orthogonal
				float3 viewDirT =
					mul(localSurface2ScaledObjectT, viewDirInObjectCoords);
				float2 offset = ParallaxOffset(-1, _Parallax, viewDirT);
				o.uv += offset;

				
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half3 bumpCol = UnpackNormal(tex2D(_BumpMap, i.uv2));
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv + bumpCol.xy * _BumpDistort);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
