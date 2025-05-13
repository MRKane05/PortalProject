Shader "Portals/PortalZeroPass"
{
	Properties
	{
		_TransparencyMask("TransparencyMask", 2D) = "white" {}
		_AlphaCutoff("Alpha Cutoff", float) = 0.1
	}
		SubShader
		{
			Tags{
				"RenderType" = "Grass"
				"Queue" = "Geometry+100"
				"IgnoreProjector" = "True"
			}
			LOD 100

			Pass
			{

				//Blend SrcAlpha OneMinusSrcAlpha
				ZWrite On
				ZTest LEqual
				Lighting Off
				Cull Back
				Offset -1, -1

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

			sampler2D _TransparencyMask;
			float4 _TransparencyMask_ST;
			fixed4 _Color;
			float _AlphaCutoff;

			float4x4 PORTAL_MATRIX_VP;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _TransparencyMask);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 portalCol = tex2D(_TransparencyMask, i.uv.xy);
				
				clip(portalCol.a - _AlphaCutoff);
				
				return fixed4(0,0,0,0);
			}
			ENDCG
		}
		}
			FallBack "Mobile/Diffuse"
}
