﻿Shader "Portals/BasicPortal"
{
	Properties
	{
		_MainTex("DefaultTexture", 2D) = "white" {}
		_LeftEyeTexture("LeftEyeTexture", 2D) = "bump" {}
		_RecurseTexture("Recursive Texture", 2D) = "bump" {}
		_TransparencyMask("TransparencyMask", 2D) = "white" {}
		_Color("Portal Tint", Color) = (1,1,1,1)
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
				float4 recScreenUV: TEXCOORD2;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _LeftEyeTexture;
			sampler2D _RecurseTexture;
			sampler2D _TransparencyMask;
			float4 _MainTex_ST;
			fixed4 _Color;

			uniform float4x4 PORTAL_MATRIX_VP;
			uniform float _samplePreviousFrame = 0;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);

				// Instead of getting the clip position of our portal from the currently rendering camera,
				// calculate the clip position of the portal from a higher level portal. PORTAL_MATRIX_VP == camera.projectionMatrix.
				float4 clipPos = mul(PORTAL_MATRIX_VP, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)));
				clipPos.y *= _ProjectionParams.x;

				//o.screenUV = lerp(ComputeScreenPos(o.vertex), ComputeScreenPos(clipPos), _samplePreviousFrame);
				o.screenUV = ComputeScreenPos(o.vertex);
				o.recScreenUV = ComputeScreenPos(clipPos);
				o.screenUV = lerp(o.screenUV, o.recScreenUV, _samplePreviousFrame);

				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				float2 sUV = i.screenUV.xy / i.screenUV.w;
				fixed4 col = tex2D(_LeftEyeTexture, sUV);
				float2 rUV = i.recScreenUV.xy / i.recScreenUV.w;
				fixed4 recCol = tex2D(_RecurseTexture, rUV);

				col = lerp(col, recCol, _samplePreviousFrame);

				fixed4 portalCol = tex2D(_TransparencyMask, i.uv.xy);	//This'll need to be a stacked image of sorts

				col.a = portalCol.a;	//Set alpha based off of image alpha
				

				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	FallBack "Mobile/Diffuse"
}
