Shader "Portals/Invisible"
{
    Properties
    {
        _MainTex("DefaultTexture", 2D) = "white" {}
        _LeftEyeTexture("LeftEyeTexture", 2D) = "bump" {}
        _TransparencyMask("TransparencyMask", 2D) = "white" {}
        _Color("Portal Tint", Color) = (1,1,1,1)
        _AlphaCutoff("Alpha Cutoff", float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            // Don't render anything
        }
    }
}