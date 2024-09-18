Shader "Custom/UnderwaterFogShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (0.1, 0.3, 0.4, 1)
        _FogStart ("Fog Start", Float) = 0
        _FogEnd ("Fog End", Float) = 100
        _FogEnabled ("Fog Enabled", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL

        Pass
        {
            Name "UnderwaterFog"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float4 _FogColor;
            float _FogStart;
            float _FogEnd;
            float _FogEnabled;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                if (_FogEnabled < 0.5) return color;

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV);
                float linearDepth = LinearEyeDepth(depth, _ZBufferParams);
                
                float fogFactor = saturate((linearDepth - _FogStart) / (_FogEnd - _FogStart));
                return lerp(color, _FogColor, fogFactor);
            }
            ENDHLSL
        }
    }
}