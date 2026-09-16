Shader "Universal 2D/PixelCircle"
{
    Properties
    {
        _Color ("Ring Color", Color) = (1, 1, 1, 1)
        _FillColor ("Fill Color", Color) = (1, 1, 1, 0.15)
        _Thickness ("Pixel Thickness", Float) = 1.0
        [Toggle] _DitherFill ("Checkerboard Dither Fill", Float) = 0
        [Toggle] _Dashed ("Dashed Ring", Float) = 0
        _DashCount ("Dash Count", Float) = 24
        [HideInInspector] _MainTex ("Sprite Texture", 2D) = "white" {}
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
        ZWrite Off
        Cull Off

        Pass
        {
            Name "PixelCirclePass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float2 posOS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _FillColor;
                float _Thickness;
                float _DitherFill;
                float _Dashed;
                float _DashCount;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                output.posOS = input.positionOS.xy;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float pixelStep = fwidth(input.posOS.x);
                if (pixelStep <= 0.0)
                {
                    pixelStep = 0.001;
                }

                float distInPixels = length(input.posOS) / pixelStep;
                float radiusInPixels = 0.5 / pixelStep - (_Thickness * 0.5);

                float diff = abs(distInPixels - radiusInPixels);

                // Mask Circle
                if (distInPixels > radiusInPixels + (_Thickness * 0.5))
                {
                    discard;
                }

                // Ring Outline
                if (diff <= _Thickness * 0.5)
                {
                    if (_Dashed > 0.5)
                    {
                        float angle = atan2(input.posOS.y, input.posOS.x);
                        if (sin(angle * _DashCount) < 0.0)
                        {
                            discard;
                        }
                    }

                    return _Color * input.color;
                }

                // Fill Circle
                if (_FillColor.a > 0.0)
                {
                    if (_DitherFill > 0.5)
                    {
                        int2 screenPixel = int2(input.positionCS.xy);
                        if ((screenPixel.x + screenPixel.y) % 2 != 0)
                        {
                            discard;
                        }
                    }

                    return _FillColor * input.color;
                }

                discard;
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
