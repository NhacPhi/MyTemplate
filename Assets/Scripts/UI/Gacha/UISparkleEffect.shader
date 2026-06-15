Shader "UI/SparkleEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Sparkle Settings)]
        _BgOpacity ("Background Opacity (Độ mờ nền)", Range(0, 1)) = 0.0
        _SparkleColor ("Sparkle Color", Color) = (1, 0.9, 0.2, 1)
        _GridSize ("Grid Size (Mật độ ô)", Float) = 15.0
        _SparkleSpeed ("Twinkle Speed (Tốc độ chớp)", Float) = 3.0
        _FloatSpeed ("Float Speed (Tốc độ bay lên)", Float) = 0.2
        _SparkleDensity ("Density (Tỉ lệ xuất hiện 0-1)", Float) = 0.4
        _SparkleSize ("Sparkle Size (Kích thước hạt)", Float) = 0.12

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
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            fixed4 _SparkleColor;
            float _BgOpacity;
            float _GridSize;
            float _SparkleSpeed;
            float _FloatSpeed;
            float _SparkleDensity;
            float _SparkleSize;

            // Hàm sinh số ngẫu nhiên
            float2 random2(float2 p)
            {
                return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
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
                // Core UI rendering logic
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                fixed4 baseColor = color;
                // Áp dụng độ mờ nền từ Shader thay vì Image Alpha
                baseColor.a *= _BgOpacity;

                // Sparkle generation logic
                float2 uv = IN.texcoord;
                uv.y -= _Time.y * _FloatSpeed;
                uv *= _GridSize;

                float2 gridId = floor(uv);
                float2 gridUv = frac(uv);

                float sparkleVal = 0.0;

                for(int y = -1; y <= 1; y++)
                {
                    for(int x = -1; x <= 1; x++)
                    {
                        float2 offset = float2(x, y);
                        float2 randVal = random2(gridId + offset);
                        
                        if (randVal.x < _SparkleDensity)
                        {
                            float2 pointPos = offset + randVal - gridUv;
                            float dist = length(pointPos);
                            
                            float twinkle = sin(_Time.y * _SparkleSpeed + randVal.y * 6.28) * 0.5 + 0.5;
                            float intensity = smoothstep(_SparkleSize, 0.0, dist) * twinkle;
                            intensity = pow(intensity, 1.5);

                            sparkleVal = max(sparkleVal, intensity);
                        }
                    }
                }

                // Final color composition
                fixed4 finalColor;
                float sparkleAlpha = sparkleVal * _SparkleColor.a;
                
                finalColor.rgb = baseColor.rgb * baseColor.a + _SparkleColor.rgb * sparkleVal * 3.0;
                finalColor.a = max(baseColor.a, sparkleAlpha);

                // Re-apply clipping to the sparkles so they don't bleed out of RectMask2D
                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}
