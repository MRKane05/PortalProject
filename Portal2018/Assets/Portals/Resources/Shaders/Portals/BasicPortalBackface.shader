Shader "Portals/BasicPortalBackface"
{
	Properties
	{
		_MainTex("DefaultTexture", 2D) = "white" {}
		_LeftEyeTexture("LeftEyeTexture", 2D) = "bump" {}
		_TransparencyMask("TransparencyMask", 2D) = "white" {}
		_Color("Portal Tint", Color) = (1,1,1,1)
		_AlphaCutoff("Alpha Cutoff", float) = 0.1
	}
	SubShader
	{
		Tags{
			"RenderType" = "Opaque"
			"Queue" = "Transparent+101"
			"IgnoreProjector" = "True"
		}
		LOD 100

		Pass
		{
			Blend One Zero
			ZWrite Off
			ZTest Always
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
				float4 objPos : TEXCOORD2;
				float4 reconUV : TEXCOORD3;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _LeftEyeTexture;
			sampler2D _TransparencyMask;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _AlphaCutoff;

			//Our details for recursive portal rendering
			uniform float4 offset_close;
			uniform float4 offset_far;
			uniform int portal_rec = 7;


			float4 reconstructFrontFaceUV(float4 objPos) {
				float3 objSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
				float3 camToVertex = objSpaceCameraPos - objPos.xyz;
				// Solve for z = 0
				// camPos.z + toVertex.z * t = 0
				// t = -camPos.z / toVertex.z
				float t = -objSpaceCameraPos.z / camToVertex.z;
				float2 uv = objSpaceCameraPos.xy + t * camToVertex.xy + 0.5;
				return float4(uv, 0, 1);
			}

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				o.screenUV = ComputeScreenPos(o.vertex);
				o.objPos = v.vertex;
				o.reconUV = reconstructFrontFaceUV(v.vertex);

				return o;
			}

			fixed4 getTextureMap(float2 baseUV, float4 offset) {
				//baseUV += offset_close.xy;
				return tex2D(_LeftEyeTexture, (baseUV - offset.xy) * offset.z + offset.xy);
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				float2 sUV = i.screenUV.xy / i.screenUV.w;
				fixed4 col = tex2D(_LeftEyeTexture, sUV);

				for (int t = 0; t < portal_rec; t++) {
					//Technically we need to apply a fog effect to this
					fixed4 backCol = getTextureMap(sUV, lerp(offset_close, offset_far, t / (float)portal_rec));
					col = lerp(backCol, col, col.a);
				}

				//float4 reconUV = reconstructFrontFaceUV(i.objPos);	//Frustratingly this has to be done here to avoid vertex warping effects
				fixed4 portalCol = tex2D(_TransparencyMask, i.reconUV.xy);
				fixed4 portalTerminusCol = tex2D(_MainTex, i.reconUV.xy);// *_Color;
				//col = lerp(portalTerminusCol, col, col.a * portalViewAlpha);
				clip(portalCol.a - _AlphaCutoff);
				
				col.rgb += portalCol.rgb * _Color.rgb;	//Put a glow on the border

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	FallBack "Mobile/Diffuse"
}
