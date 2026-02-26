Shader "Custom/SimpleSoulStream"
{
    Properties
    {
        [Header(Main Settings)]
        [HDR] _Color ("Energy Color", Color) = (0.2, 1.0, 0.8, 1) // Màu xanh linh hồn
        _MainTex ("Texture Mask (Optional)", 2D) = "white" {} // Dùng để mask hình dạng nếu cần
        
        [Header(Flow Settings)]
        _Speed ("Flow Speed", Float) = 3.0           // Tốc độ bay
        _NoiseScale ("Noise Scale (X, Y)", Vector) = (2.0, 5.0, 0, 0) // Độ giãn của vân khí
        _StreamWidth ("Stream Width", Range(0, 1)) = 0.5 // Độ rộng dòng chảy
        _Taper ("Taper Amount", Range(0, 2)) = 1.0   // Độ thu nhỏ đầu hút (1 = nhọn hoắt)
        
        [Header(Fading)]
        _FadeSoftness ("Edge Softness", Range(0.01, 0.5)) = 0.2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha One // Additive blend (Sáng rực rỡ)
        Cull Off 
        Lighting Off 
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 noiseUV : TEXCOORD1;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Speed;
            float4 _NoiseScale;
            float _StreamWidth;
            float _Taper;
            float _FadeSoftness;

            // --- HÀM NOISE NHANH (Gradient Noise) ---
            float2 hash(float2 p) {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }
            float noise(float2 p) {
                const float K1 = 0.366025404; const float K2 = 0.211324865;
                float2 i = floor(p + (p.x + p.y) * K1);
                float2 a = p - i + (i.x + i.y) * K2;
                float2 o = (a.x > a.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                float2 b = a - o + K2; float2 c = a - 1.0 + 2.0 * K2;
                float3 h = max(0.5 - float3(dot(a, a), dot(b, b), dot(c, c)), 0.0);
                float3 n = h * h * h * h * float3(dot(a, hash(i + 0.0)), dot(b, hash(i + o)), dot(c, hash(i + 1.0)));
                return dot(n, float3(70.0, 70.0, 70.0));
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;

                // Tính toán UV cho Noise để nó trôi đi
                // Chúng ta kéo giãn UV.x nhiều hơn để tạo vệt dài (motion blur)
                o.noiseUV = v.uv * _NoiseScale.xy;
                o.noiseUV.x -= _Time.y * _Speed; // Trôi theo trục X (ngang)

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. TẠO HÌNH DÁNG DÒNG CHẢY (Stream Shape)
                // Chúng ta muốn dòng chảy hẹp dần về phía bên phải (UV.x = 1)
                // taperFactor càng lớn về phía cuối dòng
                float taperFactor = 1.0 - (i.uv.x * _Taper); 
                taperFactor = max(0.1, taperFactor); // Không để nó bằng 0

                // Điều chỉnh độ rộng dòng chảy dựa trên độ thon (taper)
                float currentWidth = _StreamWidth * taperFactor;
                
                // Tính khoảng cách từ tâm dòng chảy (UV.y = 0.5)
                float distY = abs(i.uv.y - 0.5);
                
                // Tạo mask mờ biên cho dòng chảy
                float streamMask = smoothstep(currentWidth, currentWidth - _FadeSoftness, distY);

                // 2. TẠO VÂN KHÍ (NOISE)
                float n = noise(i.noiseUV);
                n = 0.5 + 0.5 * n; // Chuyển về khoảng 0-1
                
                // Làm noise sắc nét hơn
                n = smoothstep(0.3, 0.8, n);

                // 3. XỬ LÝ ĐẦU VÀ ĐUÔI (FADE IN/OUT)
                // Mờ dần ở 2 đầu trái phải để không bị cắt cụt
                float endFade = smoothstep(0.0, 0.2, i.uv.x) * smoothstep(1.0, 0.8, i.uv.x);

                // 4. TỔNG HỢP
                fixed4 col = tex2D(_MainTex, i.uv); // Lấy texture gốc (nếu có)
                
                float finalAlpha = n * streamMask * endFade * i.color.a;
                
                // Màu sắc sáng rực (Additive)
                float3 finalColor = i.color.rgb * (n + 0.5) * 2.0; 

                return fixed4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}