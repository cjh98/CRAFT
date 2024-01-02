Shader "Custom/SimpleFog"
{
    Properties
    {
        _FogRadius("Fog Radius", Float) = 10
        _FogColor("Fog Color", Color) = (.5, .5, .5, 1)
    }

        CGINCLUDE
#include "UnityCG.cginc"
        ENDCG

        SubShader
    {
        Tags { "Queue" = "Overlay" }

        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            INTERNAL_DATA
        };

        float _FogRadius;
        fixed4 _FogColor;

        void surf(Input IN, inout SurfaceOutput o)
        {
            // Add fog based on distance from the camera
            float fogAmount = saturate(1.0 - length(IN.viewDir) / _FogRadius);
            o.Albedo = _FogColor.rgb;
            o.Alpha = lerp(1, o.Alpha, fogAmount); // Adjust the alpha based on fog amount
        }
        ENDCG
    }

        Fallback "Diffuse"
}