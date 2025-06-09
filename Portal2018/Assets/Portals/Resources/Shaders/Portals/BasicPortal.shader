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

			sampler2D _DefaultTexture;
			sampler2D _LeftEyeTexture;
			sampler2D _RecurseTexture;
			sampler2D _TransparencyMask;
			float4 _DefaultTexture_ST;
			fixed4 _Color;

			uniform float4x4 PORTAL_MATRIX_VP;
			uniform float _samplePreviousFrame = 0;
			uniform float4 _defaultTextureOffset = float4(0.2, 0.3, 0.6, 0.7);
			uniform float _portalFade = 0;

			fixed _AlphaCutoff;
			fixed _FlameCutoff;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _DefaultTexture);
				UNITY_TRANSFER_FOG(o,o.vertex);

				// Instead of getting the clip position of our portal from the currently rendering camera,
				// calculate the clip position of the portal from a higher level portal. PORTAL_MATRIX_VP == camera.projectionMatrix.
				float4 clipPos = mul(PORTAL_MATRIX_VP, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)));
				clipPos.y *= _ProjectionParams.x;

				//o.screenUV = lerp(ComputeScreenPos(o.vertex), ComputeScreenPos(clipPos), _samplePreviousFrame);
				o.screenUV = ComputeScreenPos(o.vertex);
				o.recScreenUV = ComputeScreenPos(clipPos);
				o.screenUV = lerp(o.screenUV, o.recScreenUV, _samplePreviousFrame);

				//_defaultTextureOffset = float4(_Time.y, _Time.y, _Time.y, _Time.y);

				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 defaultCol = tex2D(_DefaultTexture, i.uv.xy * 2.0 + _defaultTextureOffset.xy);
				fixed2 UVDistort = (defaultCol.xz - 0.5) * 0.4;
				defaultCol *= tex2D(_DefaultTexture, i.uv.xy*1.0 + _defaultTextureOffset.zw + UVDistort.xy);	//Get our composite mapping mask
				//defaultCol *= 0.5; //Average the above two textures for combination later

				//We might use the above textures to convolue our sampling of the portal outline
				fixed4 portalCol = tex2D(_TransparencyMask, i.uv.xy + UVDistort.xy*0.05);	//This'll need to be a stacked image of sorts
				
				fixed portalFlameBorder = saturate(defaultCol.b + portalCol.y - (0.2 + _portalFade) + portalCol.r);
				//portalFlameBorder = saturate(portalFlameBorder + portalCol.r);

				fixed3 portalFlames = _Color.rgb * portalFlameBorder;

				clip(portalCol.a - _AlphaCutoff);	//Clip to toss out complicated calculations

				float2 sUV = i.screenUV.xy / i.screenUV.w;
				fixed4 col = tex2D(_LeftEyeTexture, sUV);
				float2 rUV = i.recScreenUV.xy / i.recScreenUV.w;
				fixed4 recCol = tex2D(_RecurseTexture, rUV);

				col = lerp(col, recCol, _samplePreviousFrame);

				//Create a blanker for the entire col setup
				col.rgb = lerp(_Color.rgb * 0.8, col.rgb, _portalFade);

				col.rgb = lerp(col.rgb, portalFlames.rgb, portalFlameBorder);

				//So our effective _FlameCutoff range for the above blend is about 0.2 to 1.2 and as it gets lower we need to blend in significantly less of our
				//visible texture

				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	FallBack "Mobile/Diffuse"
	}
