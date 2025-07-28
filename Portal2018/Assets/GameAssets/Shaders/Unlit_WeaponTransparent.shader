Shader "Vita/Unlit_WeaponTransparent" {
    Properties{
      _MainTex("Diffuse Texture", 2D) = "white" {}
      _Color("Main Color", Color) = (1,1,1,1)
      _Ambient("Ambient Color", Color) = (1,1,1,1)
      //_Tint("Reflection Tint", Color) = (1,1,1,1)
      //_Cube("Cubemap", CUBE) = "" {}
    }

        SubShader{
            Tags { "RenderType" = "Transparent" 
            "Queue" = "Transparent+200"
            "IgnoreProjector" = "True"}
            LOD 100

            Pass {

                ZWrite Off
                ZTest Always
                Cull Back
                Blend SrcAlpha OneMinusSrcAlpha

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
                };

                sampler2D _MainTex;
                //samplerCUBE _Cube;
                //fixed4 _Tint;
                float4 _MainTex_ST;
                fixed4 _Color;
                fixed4 _Ambient;

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    /*
                    // Transform normal to world space
                    float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                    
                    //Reflection probe sampler that's not quite up to scratch
                    float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    // compute world space view direction
                    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                    // world space normal
                    //float3 worldNormal = UnityObjectToWorldNormal(IN.normal);
                    // world space reflection vector
                    o.worldRefl = reflect(-worldViewDir, worldNormal);
                    */
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    fixed4 tex = tex2D(_MainTex, i.uv);
                    tex = tex * 0.33 + tex * _Color;
                    //tex.rgb *= i.lighting;

                    /*
                    half4 skyData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, i.worldRefl, _Roughness);
                    half3 skyColor = DecodeHDR(skyData, unity_SpecCube0_HDR);

                    tex.rgb += skyColor.rgb;*/
                    //half4 reflcol = texCUBE(_Cube, i.worldRefl);
                    //tex.rgb += reflcol.rgb * _Tint.rgb * tex.a;

                    return tex;
                }
                ENDCG
            }
      }
}