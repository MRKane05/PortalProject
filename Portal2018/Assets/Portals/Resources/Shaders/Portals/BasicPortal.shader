	Shader "Portals/BasicPortal"
	{
	Properties
	{
		_DefaultTexture("DefaultTexture", 2D) = "white" {}
		_LeftEyeTexture("LeftEyeTexture", 2D) = "bump" {}
		_RecurseTexture("Recursive Texture", 2D) = "bump" {}
		_TransparencyMask("TransparencyMask", 2D) = "white" {}
		_Color("Portal Tint", Color) = (1,1,1,1)
		_AlphaCutoff("Alpha Cutoff", float) = 0.1
		_FlameCutoff("Flames Cutoff", float) = 0.5
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
				half2 uv : TEXCOORD0;  // Use half precision
			};

			struct v2f
			{
				half2 uv : TEXCOORD0;
				half4 screenUV : TEXCOORD1;     // Use half precision
				half4 recScreenUV: TEXCOORD2;   // Use half precision
				// UNITY_FOG_COORDS(1) // Removed for performance
				float4 vertex : SV_POSITION;
			};

			sampler2D _DefaultTexture;
			sampler2D _LeftEyeTexture;
			sampler2D _RecurseTexture;
			sampler2D _TransparencyMask;
			float4 _DefaultTexture_ST;
			fixed4 _Color;

			uniform float4x4 PORTAL_MATRIX_VP;
			uniform half _samplePreviousFrame = 0;  // Use half precision
			uniform half4 _defaultTextureOffset = half4(0.2, 0.3, 0.6, 0.7);  // Use half precision
			uniform half _portalFade = 0;  // Use half precision

			fixed _AlphaCutoff;
			fixed _FlameCutoff;
			
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _DefaultTexture);
				// UNITY_TRANSFER_FOG(o,o.vertex); // Removed for performance

				// Optimize matrix calculations
				half4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				half4 clipPos = mul(PORTAL_MATRIX_VP, worldPos);
				clipPos.y *= _ProjectionParams.x;

				o.screenUV = ComputeScreenPos(o.vertex);
				o.recScreenUV = ComputeScreenPos(clipPos);

				// Single lerp operation instead of multiple assignments
				o.screenUV = lerp(o.screenUV, o.recScreenUV, _samplePreviousFrame);

				return o;
			}
			
			half4 frag(v2f i) : SV_Target
			{
				// Reduce texture sampling complexity
				half4 defaultCol = tex2D(_DefaultTexture, i.uv.xy * 2.0 + _defaultTextureOffset.xy);
				half2 UVDistort = (defaultCol.xz - 0.5) * 0.2; // Reduce distortion intensity

				// Combine operations to reduce ALU instructions
				defaultCol *= tex2D(_DefaultTexture, i.uv.xy + _defaultTextureOffset.zw + UVDistort.xy);

				// Simplified portal mask sampling
				half4 portalCol = tex2D(_TransparencyMask, i.uv.xy + UVDistort.xy * 0.05); // Reduce distortion

				// Optimize flame border calculation
				half portalFlameBorder = saturate(defaultCol.b + portalCol.y - (_portalFade*1.0) + portalCol.r);
				half3 portalFlames = _Color.rgb * (portalFlameBorder + portalCol.b);

				// Early discard for performance
				clip(portalCol.a - _AlphaCutoff);

				// Optimize screen UV calculations
				half2 sUV = i.screenUV.xy / i.screenUV.w;
				half4 col = tex2D(_LeftEyeTexture, sUV);
				half2 rUV = i.recScreenUV.xy / i.recScreenUV.w;
				half4 recCol = tex2D(_RecurseTexture, rUV);

				// Combine lerp operations
				col = lerp(col, recCol, _samplePreviousFrame);
				col.rgb = lerp(_Color.rgb * 0.8, col.rgb, saturate(_portalFade*1.1));
				col.rgb = lerp(col.rgb, portalFlames.rgb, portalFlameBorder);

				// UNITY_APPLY_FOG(i.fogCoord, col); // Removed for performance
				return col;
			}
			ENDCG
		}
	}
	FallBack "Mobile/Diffuse"
	}
