Shader "Custom/SpriteSmokeBubbles_OnlyEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        // Màu Tint này sẽ ảnh hưởng đến toàn bộ hiệu ứng
        _Color ("Tint", Color) = (1,1,1,1) 
        
        [Header(Smoke Settings)]
        // Chỉnh Alpha (A) để khói mờ hay đậm
        _SmokeColor ("Smoke Color", Color) = (0.1, 0.8, 0.2, 0.5) 
        _SmokeScale ("Smoke Scale", Float) = 5.0
        _SmokeSpeed ("Smoke Speed Y", Float) = 0.5
        _SmokeDensity ("Smoke Visibility", Range(0, 1)) = 0.7

        [Header(Bubble Settings)]
        // Chỉnh Alpha (A) để bong bóng mờ hay đậm
        _BubbleColor ("Bubble Color", Color) = (0.2, 1.0, 0.8, 0.8)
        _BubbleScale ("Bubble Size", Float) = 15.0
        _BubbleSpeed ("Bubble Speed Y", Float) = 1.2 
        _BubbleAmount ("Bubble Amount", Range(0, 1)) = 0.3             
        _PopHeight ("Pop Height Range", Range(0, 1)) = 0.7
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

        Cull Off
        Lighting Off
        ZWrite Off
        // Chế độ hòa trộn chuẩn cho vật thể trong suốt
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            // sampler2D _MainTex; // Không cần texture gốc nữa

            float4 _SmokeColor;
            float _SmokeScale;
            float _SmokeSpeed;
            float _SmokeDensity;

            float4 _BubbleColor;
            float _BubbleScale;
            float _BubbleSpeed;
            float _BubbleAmount;
            float _PopHeight;

            // --- CÁC HÀM NOISE (Giữ nguyên) ---
            float hash(float2 p) { return frac(1e4 * sin(17.0 * p.x + p.y * 0.1) * (0.1 + abs(sin(p.y * 13.0 + p.x)))); }
            float noise(float2 x) {
                float2 i = floor(x); float2 f = frac(x);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i), hash(i + float2(1.0, 0.0)), u.x),
                            lerp(hash(i + float2(0.0, 1.0)), hash(i + float2(1.0, 1.0)), u.x), u.y);
            }
            void rotate2D(inout float2 v, float r) { float c = cos(r); float s = sin(r); v = float2(v.x * c - v.y * s, v.x * s + v.y * c); }
            float fbm(float2 x) {
                float v = 0.0; float a = 0.5; float2 shift = float2(100, 100);
                rotate2D(x, 0.5); 
                for (int i = 0; i < 4; ++i) { v += a * noise(x); x = x * 2.0 + shift; a *= 0.5; }
                return v;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                // Lấy màu tint từ Sprite Renderer
                OUT.color = IN.color * _Color; 
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // --- 1. TÍNH TOÁN GIÁ TRỊ NOISE ---
                
                // Khói
                float2 smokeUV = IN.texcoord * _SmokeScale;
                smokeUV.y -= _Time.y * _SmokeSpeed; 
                smokeUV.x += sin(_Time.y * 0.5 + smokeUV.y) * 0.2; 
                float smokeVal = fbm(smokeUV);
                smokeVal = smoothstep(1.0 - _SmokeDensity, 1.0, smokeVal);
                smokeVal *= smoothstep(1.0, 0.6, IN.texcoord.y); // Mờ dần ở đỉnh

                // Bong bóng
                float2 bubbleUV = IN.texcoord * _BubbleScale;
                bubbleUV.y -= _Time.y * _BubbleSpeed;
                bubbleUV.x += sin(_Time.y * 2.0 + bubbleUV.y * 3.0) * 0.1;
                float bubbleRaw = noise(bubbleUV);
                float bubbleShape = smoothstep(1.0 - _BubbleAmount, 1.0 - _BubbleAmount + 0.05, bubbleRaw);
                float popNoise = noise(IN.texcoord * 10.0 + _Time.y * 3.0);
                float popMask = step(smoothstep(_PopHeight, _PopHeight + 0.2, IN.texcoord.y), popNoise);
                float finalBubbleVal = bubbleShape * popMask;

                // --- 2. TÍNH TOÁN MÀU VÀ ALPHA CUỐI CÙNG ---

                // Tính cường độ Alpha thực tế của khói và bong bóng (dựa trên settings)
                float smokeAlpha = smokeVal * _SmokeColor.a;
                float bubbleAlpha = finalBubbleVal * _BubbleColor.a;

                // Alpha cuối cùng là tổng hợp của cả hai, tối đa là 1.
                // Chỗ nào không có khói hay bong bóng, alpha sẽ là 0 (trong suốt).
                float finalAlpha = saturate(smokeAlpha + bubbleAlpha);

                // Nếu pixel này hoàn toàn trong suốt, bỏ qua không vẽ.
                if (finalAlpha < 0.01) discard;

                // Tính màu RGB cuối cùng. 
                // Ưu tiên hiển thị màu bong bóng đè lên màu khói.
                // Sử dụng lerp dựa trên tỷ lệ đóng góp của bong bóng vào alpha tổng.
                float3 finalRGB = lerp(_SmokeColor.rgb, _BubbleColor.rgb, bubbleAlpha / (finalAlpha + 0.0001));
                
                // Nhân thêm với màu Tint của Sprite Renderer (nếu có)
                finalRGB *= IN.color.rgb;
                finalAlpha *= IN.color.a;

                return fixed4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
}