Shader "Unlit/LightProbeDualDiffuse"
{
Properties{
    _MainTex("Diffuse Texture", 2D) = "white" {}
    _Color("Main Color", Color) = (1,1,1,1)
}

    SubShader{
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 lighting : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            // Spherical Harmonics coefficients passed from script
            float4 _SHAr;
            float4 _SHAg;
            float4 _SHAb;
            float4 _SHBr;
            float4 _SHBg;
            float4 _SHBb;
            float4 _SHC;

            // Second set of SH coefficients for interpolation
            float4 _SHAr2;
            float4 _SHAg2;
            float4 _SHAb2;
            float4 _SHBr2;
            float4 _SHBg2;
            float4 _SHBb2;
            float4 _SHC2;

            // Interpolation parameter
            float _LightT;

            // Custom SH evaluation function (same as Unity's ShadeSH9)
            float3 EvaluateSH(float3 normal, float4 SHAr, float4 SHAg, float4 SHAb,
                            float4 SHBr, float4 SHBg, float4 SHBb, float4 SHC) {
                float3 x1, x2, x3;

                // Linear + constant polynomial terms
                x1.r = dot(SHAr, float4(normal, 1.0));
                x1.g = dot(SHAg, float4(normal, 1.0));
                x1.b = dot(SHAb, float4(normal, 1.0));

                // 4 of the quadratic polynomials
                float4 vB = normal.xyzz * normal.yzzx;
                x2.r = dot(SHBr, vB);
                x2.g = dot(SHBg, vB);
                x2.b = dot(SHBb, vB);

                // Final quadratic polynomial
                float vC = normal.x * normal.x - normal.y * normal.y;
                x3 = SHC.rgb * vC;

                return x1 + x2 + x3;
            }

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Transform normal to world space
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);

                // Evaluate first set of spherical harmonics
                float3 lighting1 = EvaluateSH(worldNormal, _SHAr, _SHAg, _SHAb, _SHBr, _SHBg, _SHBb, _SHC);

                // Evaluate second set of spherical harmonics
                float3 lighting2 = EvaluateSH(worldNormal, _SHAr2, _SHAg2, _SHAb2, _SHBr2, _SHBg2, _SHBb2, _SHC2);

                // Interpolate between the two lighting results
                o.lighting = lerp(lighting1, lighting2, _LightT);

                // Ensure lighting is never completely black
                o.lighting = max(o.lighting, float3(0.05, 0.05, 0.05));

                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
                tex.rgb *= i.lighting;
                return tex;
            }
            ENDCG
        }
    }
}