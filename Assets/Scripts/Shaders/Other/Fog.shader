Shader "Custom/FogShader"
{
    Properties
    {
        _Color("Fog Color", Color) = (.5, .6, .7, 1)
        _FogDensity("Fog Density", Range(0, 1)) = 0.1
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;
        half _FogDensity;

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = _Color; // Use _Color directly, no need for texture in this example
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            // Calculate fog based on distance
            float fogFactor = exp2(-_FogDensity * IN.eyeDepth);
            o.Albedo = lerp(_Color.rgb, _Color.rgb, fogFactor);
        }
        ENDCG
    }
        FallBack "Diffuse"
}