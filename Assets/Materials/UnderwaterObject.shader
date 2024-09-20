Shader "Custom/UnderwaterObject"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _ColorShift ("Color Shift Intensity", Range(0,1)) = 0.5
        _DarkenIntensity ("Darken Intensity", Range(0,1)) = 0.3
        _AmbientLight ("Ambient Light Intensity", Range(0,1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf UnderwaterLighting fullforwardshadows
        #pragma target 3.0

        #include "UnityPBSLighting.cginc"

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _ColorShift;
        float _DarkenIntensity;
        float _AmbientLight;

        uniform float4 _UnderwaterFogColor;
        uniform float _WaterSurfaceY;
        uniform float _MaxDepth;
        uniform float _MaxFogDensity;

        struct UnderwaterSurfaceOutput
        {
            fixed3 Albedo;
            fixed3 Normal;
            half3 Emission;
            half Metallic;
            half Smoothness;
            half Occlusion;
            fixed Alpha;
            float Depth;
        };

        // Custom lighting function
        half4 LightingUnderwaterLighting(UnderwaterSurfaceOutput s, half3 viewDir, UnityGI gi)
        {
            SurfaceOutputStandard r;
            r.Albedo = s.Albedo;
            r.Normal = s.Normal;
            r.Emission = s.Emission;
            r.Metallic = s.Metallic;
            r.Smoothness = s.Smoothness;
            r.Occlusion = s.Occlusion;
            r.Alpha = s.Alpha;

            float depthRatio = saturate(s.Depth / (_WaterSurfaceY - _MaxDepth));

            // Attenuate direct light based on depth
            gi.light.color = lerp(gi.light.color, _UnderwaterFogColor.rgb, depthRatio);
            gi.light.color *= (1 - depthRatio * 0.9); // Reduce intensity with depth

            // Standard lighting calculation
            half4 c = LightingStandard(r, viewDir, gi);

            // Soften shadows based on depth
            half3 shadowColor = _UnderwaterFogColor.rgb * _AmbientLight;
            c.rgb = lerp(c.rgb, shadowColor, depthRatio * 0.8);

            // Add depth-based ambient light
            c.rgb += _UnderwaterFogColor.rgb * _AmbientLight * depthRatio;

            return c;
        }

        void LightingUnderwaterLighting_GI(UnderwaterSurfaceOutput s, UnityGIInput data, inout UnityGI gi)
        {
            UNITY_GI(gi, s, data);
        }

        float3 UnderwaterShift(float3 originalColor, float3 shiftColor, float depthRatio)
        {
            float3 shiftedColor = lerp(originalColor, shiftColor, _ColorShift * depthRatio);
            float luminance = dot(shiftedColor, float3(0.299, 0.587, 0.114));
            shiftedColor = lerp(shiftedColor, float3(luminance, luminance, luminance), 0.2 * depthRatio);
            return shiftedColor;
        }

        void surf (Input IN, inout UnderwaterSurfaceOutput o)
        {
            float depth = max(0, _WaterSurfaceY - IN.worldPos.y);
            float depthRatio = saturate(depth / (_WaterSurfaceY - _MaxDepth));

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            c.rgb = UnderwaterShift(c.rgb, _UnderwaterFogColor.rgb, depthRatio);
            c.rgb *= 1 - (_DarkenIntensity * depthRatio);

            o.Albedo = c.rgb;
            o.Metallic = _Metallic * (1 - depthRatio * 0.5);
            o.Smoothness = _Glossiness * (1 - depthRatio * 0.7);
            o.Alpha = c.a;
            o.Normal = float3(0,0,1);
            o.Occlusion = 1;

            float fogFactor = 1 - exp(-_MaxFogDensity * depth);
            o.Emission = lerp(float3(0,0,0), _UnderwaterFogColor.rgb, fogFactor * 0.3);

            o.Depth = depth;
        }
        ENDCG
    }
    FallBack "Diffuse"
}