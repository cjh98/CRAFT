Shader "Custom/DefaultShader"
{
    Properties
    {
        _MainTexArray("Texture Array", 2DArray) = "" { }
    }

        SubShader
    {
        Tags {"Queue" = "Overlay" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        UNITY_DECLARE_TEX2DARRAY(_MainTexArray);

        struct Input
        {
            float3 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            // Use the z component of UV to sample the correct texture
            o.Albedo = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, IN.uv_MainTex).rgb;
        }
        ENDCG
    }

        FallBack "Diffuse"
}
