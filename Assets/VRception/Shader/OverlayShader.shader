Shader "Custom/OverlayShader" 
{
    Properties 
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    }

    SubShader 
    {
        Tags 
        {
            "Queue"="Overlay+2000"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        Pass 
        {
            Lighting Off 
            Cull Off 
            ZWrite On
            ZTest Always
            ColorMask 0
        }

        UsePass "Transparent/Diffuse/FORWARD"
    }
    Fallback "Transparent/VertexLit"
}