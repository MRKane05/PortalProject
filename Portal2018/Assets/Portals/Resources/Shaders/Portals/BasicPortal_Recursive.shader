Shader "Portals/BasicPortal"
{
	Properties
	{
		_MainTex("DefaultTexture", 2D) = "white" {}
		_LeftEyeTexture("LeftEyeTexture", 2D) = "bump" {}
		_TransparencyMask("TransparencyMask", 2D) = "white" {}
		_Color("Portal Tint", Color) = (1,1,1,1)
		//cpy_offset("Copy Offsets", Vector) = (0,0,1,1)
	}
	SubShader
	{
		Tags{
			"RenderType" = "Opaque"
			"Queue" = "Geometry+100"
			"IgnoreProjector" = "True"
		}
		LOD 100

		Pass
		{

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			Lighting Off
			Cull Back

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
				float4 screenUV : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _LeftEyeTexture;
			sampler2D _TransparencyMask;
			float4 _MainTex_ST;
			fixed4 _Color;

			uniform float4 cpy_offset;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				o.screenUV = ComputeScreenPos(o.vertex);
				//Calc screenspace (more potential optimisation here later)
				/*
				//Section for handling recursive sampling
				float4 clipPos = mul(PORTAL_MATRIX_VP, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)));
				clipPos.y *= _ProjectionParams.x;
				//clipPos.z = 1;
				o.screenUV = ComputeScreenPos(clipPos);
				*/
				return o;
			}

			fixed4 getTextureMap(float2 baseUV, float4 offset, float multiplier) {
				//baseUV += cpy_offset.xy;
				return tex2D(_LeftEyeTexture, (baseUV - cpy_offset.xy) * cpy_offset.z * multiplier + cpy_offset.xy);
			}
			
			/*
							for (float t = 1.0; t < 4.0; t++) {
					fixed4 backCol = getTextureMap(sUV, cpy_offset, t);
					col = lerp(backCol, col, col.a);
				}
				*/

			fixed4 frag(v2f i) : SV_Target
			{
				float2 sUV = i.screenUV.xy / i.screenUV.w;
				fixed4 col = tex2D(_LeftEyeTexture, sUV);
				//First portal iteration
				/*
				fixed4 backCol = getTextureMap(sUV, cpy_offset, 1.0);
				col = lerp(backCol, col, col.a);*/

				for (float t = 1.0; t < 4.0; t++) {
					fixed4 backCol = getTextureMap(sUV, cpy_offset, t);
					col = lerp(backCol, col, col.a);
				}

				/*
				//Second portal iteration
				backCol = getTextureMap(sUV, cpy_offset, 2.0);
				col = lerp(backCol, col, col.a);
				//Third...I think you get the idea
				backCol = getTextureMap(sUV, cpy_offset, 3.0);
				col = lerp(backCol, col, col.a);
				*/
				fixed4 portalCol = tex2D(_TransparencyMask, i.uv.xy);

				col.a = portalCol.a;	//Set alpha based off of image alpha
				col.rgb += portalCol.rgb * _Color.rgb;	//Put a glow on the border
				//col = portalCol;
				// sample the texture
				//i.screenUV /= i.screenUV.w;
				//fixed4 col = tex2Dproj(_LeftEyeTexture, i.screenUV); //tex2D(_MainTex, i.uv);
				//fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	FallBack "Mobile/Diffuse"
}
