// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Mobile/Particles/MultiplySheet"
{
   Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _Tiles ("Tile Number", float) = 4
        _DepthThreshold("Depth Threshold", float) = 1
    }
    
    Category
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
        }
        
        Blend Zero SrcColor // Multiply blend mode
        ColorMask RGB
        Cull Off
        Lighting Off
        ZWrite Off
        
        SubShader
        {
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 1.0
                
                #include "UnityCG.cginc"
                
                sampler2D _MainTex;
                fixed4 _TintColor;
                fixed _Tiles;
                //UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
                fixed _DepthThreshold;

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    fixed4 color : COLOR;
                    half2 texcoord : TEXCOORD0;
                    //uint VertexID : SV_VertexID;
                };
                
                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    fixed4 color : COLOR;
                    half2 texcoord : TEXCOORD0;
                    //float2 screenUV : TEXCOORD1;
                    //float screenDepth : TEXCOORD2;
                };

                inline float4 FastScreenPos(float4 clipPos) {
                    return float4(
                        clipPos.xy * 0.5 + clipPos.w * 0.5,  // mad instruction
                        clipPos.zw);
                }

                inline float4 InlineComputeScreenPos(float4 clipPos) {
                    float4 o = clipPos * 0.5f;
                    o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
                    o.zw = clipPos.zw;
                    return o;
                }
                
                v2f vert (appdata_t v)
                {
                    v2f o;
                    
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = fixed4(1, v.color.g, v.color.b, v.color.a) * _TintColor;
                    //We need to change the uv mapping based off of the tile number and the index of the particle
                    fixed particle_id = floor(v.color.r * 15.0); //fixed particle_id = floor(v.color.r * 16);// v.VertexID / 4;
                    fixed xPos = particle_id - floor(particle_id / _Tiles) * _Tiles;
                    fixed yPos = floor(particle_id / _Tiles);

                    fixed2 tileSize = fixed2(1.0 / _Tiles, 1.0 / _Tiles);
                    o.texcoord = v.texcoord * tileSize + fixed2(xPos * tileSize.x, yPos * tileSize.y);
                    /*
                    float4 screenPos = ComputeScreenPos(o.vertex);
                    o.screenUV = screenPos.xy / screenPos.w;  // Do divide in vertex shader
                    o.screenDepth = screenPos.z / screenPos.w; // Also pre-calculate depth
                    */
                    return o;
                }


                
                fixed4 frag (v2f i) : SV_Target
                {
                    /*
                    float sceneDepth = tex2D(_CameraDepthTexture, i.screenUV);
                    sceneDepth = LinearEyeDepth(sceneDepth);

                    float decalDepth = LinearEyeDepth(i.screenDepth);

                    // Check depth difference
                    if (sceneDepth - decalDepth > _DepthThreshold) {
                        discard;
                    }*/

                    fixed4 tex = tex2D(_MainTex, i.texcoord);
                    fixed3 col = i.color.rgb * tex.rgb;
                    
                    // Lerp between white and color based on alpha
                    col = lerp(fixed3(1,1,1), col.rgb, i.color.a * tex.a);
                    
                    return fixed4(col, 1);
                }
                ENDCG
            }
        }
    }
}