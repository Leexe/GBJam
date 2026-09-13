Shader "Fullscreen/ColorPalette"
{
    Properties
    {
        [HideInInspector]
        _MainTex ("Source Texture", 2D) = "white" {}
        _ColorRamp ("Color Ramp", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ColorPalettePass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_ColorRamp);
            SAMPLER(sampler_ColorRamp);

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                half4 screenColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);

                half luminance = saturate(dot(screenColor.rgb, half3(0.299, 0.587, 0.114)));
                half4 paletteColor = SAMPLE_TEXTURE2D(_ColorRamp, sampler_PointClamp, float2(luminance, 0.5));

                return half4(paletteColor.rgb, screenColor.a);
            }
            ENDHLSL
        }
    }
}
