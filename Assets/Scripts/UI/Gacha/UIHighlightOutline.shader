Shader "UI/HighlightOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Outline Settings (Vien Bao Ngoai))]
        [Toggle] _UseOutline ("Bat Vien Ngoai", Float) = 1
        _OutlineColor ("Mau Vien", Color) = (1, 1, 0, 1)
        _OutlineWidth ("Do Day Vien", Range(0, 50)) = 5.0
        
        [Header(Inner Glow Settings (Phat Sang Vien Trong))]
        [Toggle] _UseInnerGlow ("Bat Sang Vien Trong", Float) = 1
        _InnerGlowColor ("Mau Phat Sang", Color) = (1, 1, 1, 1)
        
        [Header(Pulse Animation)]
        [Toggle] _UsePulse ("Bat Hieu Ung Nhap Nhay", Float) = 0
        _PulseSpeed ("Toc Do Nhap Nhay", Float) = 3.0

        [Header(UI System Masking)]
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
        Blend SrcAlpha OneMinusSrcAlpha
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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            float _UseOutline;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            
            float _UseInnerGlow;
            fixed4 _InnerGlowColor;
            
            float _UsePulse;
            float _PulseSpeed;

            // Hàm đọc alpha, tự động trả về 0 nếu ra khỏi viền của hình (Giúp Inner Glow hoạt động cả khi viền chạm sát mép ảnh)
            float SampleAlpha(float2 uv)
            {
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return 0.0;
                return tex2D(_MainTex, uv).a;
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 originalColor = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // Nhan 3.0 de outline to ra hon so voi truoc day
                float2 texel = _MainTex_TexelSize.xy * _OutlineWidth * 3.0;

                // Lay mau 8 diem xung quanh pixel de tim vien
                float up = SampleAlpha(IN.texcoord + float2(0, texel.y));
                float down = SampleAlpha(IN.texcoord + float2(0, -texel.y));
                float left = SampleAlpha(IN.texcoord + float2(-texel.x, 0));
                float right = SampleAlpha(IN.texcoord + float2(texel.x, 0));
                
                float upLeft = SampleAlpha(IN.texcoord + float2(-texel.x, texel.y));
                float upRight = SampleAlpha(IN.texcoord + float2(texel.x, texel.y));
                float downLeft = SampleAlpha(IN.texcoord + float2(-texel.x, -texel.y));
                float downRight = SampleAlpha(IN.texcoord + float2(texel.x, -texel.y));

                float alphaSum = up + down + left + right + upLeft + upRight + downLeft + downRight;
                
                fixed4 finalColor = originalColor;

                // Tinh toan nhip dap
                float pulseMultiplier = 1.0;
                if (_UsePulse > 0.5)
                {
                    pulseMultiplier = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5) * 0.6 + 0.4;
                }

                // VE VIEN BAO QUANH (Outer Outline)
                if (_UseOutline > 0.5 && originalColor.a < 0.05 && alphaSum > 0.0)
                {
                    float outlineAlpha = saturate(alphaSum / 4.0);
                    finalColor.rgb = _OutlineColor.rgb;
                    finalColor.a = outlineAlpha * _OutlineColor.a * IN.color.a * pulseMultiplier;
                }

                // VE SANG VIEN BEN TRONG (Inner Glow/Highlight)
                if (_UseInnerGlow > 0.5 && originalColor.a > 0.9 && alphaSum < 7.9)
                {
                    float innerGlowIntensity = 1.0 - saturate(alphaSum / 8.0);
                    innerGlowIntensity *= _InnerGlowColor.a * pulseMultiplier;
                    
                    finalColor.rgb += _InnerGlowColor.rgb * innerGlowIntensity * 2.0;
                }

                // Xu ly Cat Hinh cua UI Canvas (Masking)
                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}
