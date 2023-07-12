// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Copied from builtin_shaders-2021.1.15f1/DefaultResourcesExtra/UI/UI-Default.shader
// Modified by huulong to support Brightness for ImageMaterialPropertyController

Shader "UI/Default-with-Brightness"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Brightness ("Brightness", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                half4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Brightness;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            /* Conversion functions taken from https://chilliant.com/rgb2hsv.html */

            float Epsilon = 1e-10;

            float3 RGBtoHCV(in float3 RGB)
            {
                // Based on work by Sam Hocevar and Emil Persson
                float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
                float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
                float C = Q.x - min(Q.w, Q.y);
                float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
                return float3(H, C, Q.x);
            }

            // float3 RGBtoHSV(in float3 RGB)
            // {
            //     float3 HCV = RGBtoHCV(RGB);
            //     float S = HCV.y / (HCV.z + Epsilon);
            //     return float3(HCV.x, S, HCV.z);
            // }

            float3 RGBtoHSL(in float3 RGB)
            {
                float3 HCV = RGBtoHCV(RGB);
                float L = HCV.z - HCV.y * 0.5;
                float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
                return float3(HCV.x, S, L);
            }

            float3 HUEtoRGB(in float H)
            {
                float R = abs(H * 6 - 3) - 1;
                float G = 2 - abs(H * 6 - 2);
                float B = 2 - abs(H * 6 - 4);
                return saturate(float3(R,G,B));
            }

            float3 HSLtoRGB(in float3 HSL)
            {
                float3 RGB = HUEtoRGB(HSL.x);
                float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
                return (RGB - 0.5) * C + HSL.z;
            }

            /* End of conversion functions */

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = IN.color * (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);

                // Apply brightness

                // A. Using HSV:
                // Formula adapted from SubGraph_AdjustBrightness.shadersubgraph
                // float3 hsv = RGBtoHSV(color.rgb);
                // float clampedValue = max(0.5, hsv.z);
                // float squareBrightness = pow(_Brightness, 2);
                // float brightnessFactor = clampedValue * squareBrightness;
                // color.rgb = min(color.rgb + brightnessFactor, 1);

                // Better:
                // B. Using HSL (since we have to use custom converters anyway, no advantage in using RGB -> HSV
                // converter as with shadergraphs for which it was provided)
                // HOTFIX: seems like pure white causes an issue, where HSLtoRGB(RGBtoHSL(color.rgb)) is not color.rgb,
                // changing pure white to pure black (independently of alpha). So we need to skip the operation if
                // color is white (alternatively we could check that luminance is 1, but we can only do this later),
                // since it cannot get any brighter anyway.
                // We can also skip the operation if fragment is completely transparent

                // ! CAUTION: this is still experimental, there are still issues including:
                // - if graphic is Masked, this shader doesn't work
                // - chromatic aberrations
                // - some black pixels become white too early, or don't turn white even at Brightness = 1
                if ((color.r != 1 || color.g != 1 || color.b != 1) && color.a > 0)
                {
                    float3 hsl = RGBtoHSL(color.rgb);
                    // Apply Power of 2 to Brightness parameter to delay strong brightness until higher level as,
                    // maybe counter-intuitively, brighter values need to be made even brighter so player can see the
                    // contrast.
                    const float squareBrightness = pow(_Brightness, 2);
                    // Lerp from original luminance to max (1), using square progression, for the brightening effect
                    float newLuminance = lerp(hsl.z, 1, squareBrightness);
                    // Replace Luminance with new value
                    const float3 newHsl = float3(hsl.xy, newLuminance);
                    color.rgb = HSLtoRGB(newHsl);
                }

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.rgb *= color.a;

                return color;
            }
        ENDCG
        }
    }
}
