Shader "Vita/LightProbeOnlyDiffuse_Weapon" {
    Properties{
      _MainTex("Diffuse Texture", 2D) = "white" {}
      _Color("Main Color", Color) = (1,1,1,1)
      _Roughness("Roughness", Range(0.0, 1.0)) = 0.5
      _Ambient("Ambient Color", Color) = (1,1,1,1)
    }

        SubShader{
            Tags { "RenderType" = "Opaque" 
            "Queue" = "Transparent+200"
            "IgnoreProjector" = "True"}
            LOD 100

            Pass {

                ZWrite Off
                ZTest Always
                Cull Back

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
                    half3 worldRefl : TEXCOORD2;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed4 _Color;
                fixed4 _Ambient;
                half _Roughness;

                // Spherical Harmonics coefficients passed from script
                float4 _SHAr;
                float4 _SHAg;
                float4 _SHAb;
                float4 _SHBr;
                float4 _SHBg;
                float4 _SHBb;
                float4 _SHC;

                // Custom SH evaluation function (same as Unity's ShadeSH9)
                float3 EvaluateSH(float3 normal) {
                    float3 x1, x2, x3;

                    // Linear + constant polynomial terms
                    x1.r = dot(_SHAr, float4(normal, 1.0));
                    x1.g = dot(_SHAg, float4(normal, 1.0));
                    x1.b = dot(_SHAb, float4(normal, 1.0));

                    // 4 of the quadratic polynomials
                    float4 vB = normal.xyzz * normal.yzzx;
                    x2.r = dot(_SHBr, vB);
                    x2.g = dot(_SHBg, vB);
                    x2.b = dot(_SHBb, vB);

                    // Final quadratic polynomial
                    float vC = normal.x * normal.x - normal.y * normal.y;
                    x3 = _SHC.rgb * vC;

                    return x1 + x2 + x3 + _Ambient;
                }

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    // Transform normal to world space
                    float3 worldNormal = UnityObjectToWorldNormal(v.normal);

                    // Evaluate spherical harmonics for this normal
                    o.lighting = EvaluateSH(worldNormal);

                    // Ensure lighting is never completely black
                    o.lighting = max(o.lighting, float3(0.05, 0.05, 0.05));

                    /*
                    //Reflection probe sampler that's not quite up to scratch
                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    // compute world space view direction
                    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                    // world space normal
                    //float3 worldNormal = UnityObjectToWorldNormal(IN.normal);
                    // world space reflection vector
                    o.worldRefl = reflect(-worldViewDir, worldNormal);*/

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
                    tex.rgb *= i.lighting;
                    /*
                    half4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, i.worldRefl, _Roughness);
                    half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);

                    tex.rgb += skyColor.rgb;*/

                    return tex;
                }
                ENDCG
            }
      }
}