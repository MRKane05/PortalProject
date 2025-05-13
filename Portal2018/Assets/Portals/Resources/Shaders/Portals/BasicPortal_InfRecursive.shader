Shader "Portals/BasicPortal_InRecursive"
{
	Properties
	{
		_MainTex("DefaultTexture", 2D) = "white" {}
		_LeftEyeTexture("LeftEyeTexture", 2D) = "bump" {}
		_RecursiveTexture("ResursiveTexture", 2D) = "bump" {}
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
				float4 rscreenUV : TEXCOORD2;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _LeftEyeTexture;
			sampler2D _RecursiveTexture;

			sampler2D _TransparencyMask;
			float4 _MainTex_ST;
			fixed4 _Color;

			float4x4 PORTAL_MATRIX_VP;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				o.screenUV = ComputeScreenPos(o.vertex);
				//Calc screenspace (more potential optimisation here later)
				
				//Section for handling recursive sampling
				float4 clipPos = mul(PORTAL_MATRIX_VP, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)));
				clipPos.y *= _ProjectionParams.x;
				//clipPos.z = 1;
				o.rscreenUV = ComputeScreenPos(clipPos);


				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_LeftEyeTexture, i.screenUV.xy / i.screenUV.w);

				fixed4 rcol = tex2D(_RecursiveTexture, i.rscreenUV.xy / i.rscreenUV.w);

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
