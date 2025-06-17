Shader "Vita/LightProbeOnlyDiffuse Dissolve" {
    Properties{
        _MainTex("Diffuse Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (1,1,1,1)

        //Dissolve properties
        _DissolveTexture("Dissolve Texutre", 2D) = "white" {}
        _EdgeColor("Edge Color", Color) = (1,1,1,1)
        _Amount("Amount", Range(0,1)) = 0
        //_ExplodeAmount("ExplodeAmount", Range(0, 1)) = 0
        //_EmissiveBoost("Emissive Boost", Float) = 2.0
    }

        SubShader{
            Tags { "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
            LOD 200
            Cull Back //Fast way to turn your material double-sided

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
                fixed4 _EdgeColor;

                // Spherical Harmonics coefficients passed from script
                float4 _SHAr;
                float4 _SHAg;
                float4 _SHAb;
                float4 _SHBr;
                float4 _SHBg;
                float4 _SHBb;
                float4 _SHC;

                //Dissolve details
                sampler2D _DissolveTexture;
                half _Amount;
                //half _ExplodeAmount;
                //half _EmissiveBoost;

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

                    return x1 + x2 + x3;
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

                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    fixed4 tex = tex2D(_MainTex, i.uv) * _Color;
                    tex.rgb *= i.lighting;

                    //Dissolve function
                    half dissolve_value = tex2D(_DissolveTexture, i.uv).r;
                    clip(dissolve_value - _Amount);
                    tex.rgb = lerp(tex.rgb, _EdgeColor,  step(dissolve_value - _Amount, 0.05f)); //emits white color

                    tex.a = 1; //This kind of doesn't matter as the clip function should handle it

                    return tex;
                }
                ENDCG
            }
      }
}