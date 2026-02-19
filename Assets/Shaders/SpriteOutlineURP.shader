Shader "Universal Render Pipeline/2D/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineThickness ("Outline Thickness", Range(0, 5)) = 1.0
        _AlphaThreshold ("Alpha Threshold", Range(0, 0.2)) = 0.01
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            half4 _OutlineColor;
            float _OutlineThickness;
            float _AlphaThreshold;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample main texture
                half4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half baseAlpha = mainColor.a;
                half3 baseColor = mainColor.rgb;

                // Calculate texel offsets
                float2 texelSize = _MainTex_TexelSize.xy;
                float offsetX = _OutlineThickness * texelSize.x;
                float offsetY = _OutlineThickness * texelSize.y;

                // Sample neighbor pixels (4 directions)
                half alphaE = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(offsetX, 0)).a;
                half alphaW = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-offsetX, 0)).a;
                half alphaN = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, offsetY)).a;
                half alphaS = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(0, -offsetY)).a;

                // Optional: Sample diagonal neighbors for smoother outline
                half alphaNE = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(offsetX, offsetY)).a;
                half alphaNW = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-offsetX, offsetY)).a;
                half alphaSE = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(offsetX, -offsetY)).a;
                half alphaSW = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + float2(-offsetX, -offsetY)).a;

                // Find maximum neighbor alpha
                half neighborMax = max(max(max(alphaE, alphaW), max(alphaN, alphaS)), 
                                     max(max(alphaNE, alphaNW), max(alphaSE, alphaSW)));

                // Create masks
                half insideMask = step(_AlphaThreshold, baseAlpha);
                half neighborMask = step(_AlphaThreshold, neighborMax);
                half outsideMask = 1.0 - insideMask;
                half outlineMask = outsideMask * neighborMask;

                // Blend colors
                half3 finalColor = lerp(baseColor, _OutlineColor.rgb, outlineMask);
                half finalAlpha = lerp(baseAlpha, _OutlineColor.a, outlineMask);

                return half4(finalColor * input.color.rgb, finalAlpha * input.color.a);
            }
            ENDHLSL
        }
    }
}
