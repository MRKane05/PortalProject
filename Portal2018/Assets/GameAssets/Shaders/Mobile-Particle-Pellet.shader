Shader "Mobile/Particles/Pellet" {
    Properties{
        _MainTex("Particle Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (1,1,1,1)
    }
        Category{
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
            Blend One One
            Cull Off Lighting Off ZWrite Off Fog { Color(1,1,1,1) }
                BindChannels {
                    Bind "Color", color
                    Bind "Vertex", vertex
                    Bind "TexCoord", texcoord
                }
                SubShader {
                    Pass {
                        SetTexture[_MainTex] {
                            constantColor[_Color]
                            combine texture * constant
                        }
                    }
                }
        }
}