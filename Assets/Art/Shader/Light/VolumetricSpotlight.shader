Shader "Custom/VolumetricSpotlight"
{
    Properties
    {
        [Header(Main Settings)]
        _MainColor ("Light Color", Color) = (1, 0.7, 0.3, 1) // Màu vàng cam mặc định
        _Intensity ("Intensity", Range(0, 5)) = 1.5         // Cường độ sáng

        [Header(Beam Shape)]
        _SpotAngle ("Spot Angle", Range(0, 1)) = 0.3        // Góc mở của đèn
        _Softness ("Edge Softness", Range(0, 1)) = 0.2      // Độ mờ viền
        _FadePower ("Vertical Fade", Range(0.1, 5)) = 2.0   // Độ tắt dần theo chiều dọc

        [Header(Dust Effect)]
        _NoiseScale ("Dust Scale", Float) = 15.0            // Độ chi tiết của bụi/tia sáng
        _NoiseSpeed ("Dust Speed", Float) = 0.5             // Tốc độ bay của bụi
        _RayStretch ("Ray Stretch", Float) = 2.0            // Kéo dài tia sáng
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        
        // Chế độ hòa trộn: Additive (Cộng) giúp ánh sáng rực rỡ hơn trên nền tối
        // Nếu muốn trong suốt bình thường, đổi thành: Blend SrcAlpha OneMinusSrcAlpha
        Blend SrcAlpha One 
        ZWrite Off
        Cull Off // Vẽ cả 2 mặt của Quad

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _MainColor;
            float _SpotAngle;
            float _Softness;
            float _FadePower;
            float _NoiseScale;
            float _NoiseSpeed;
            float _RayStretch;
            float _Intensity;

            // Hàm tạo nhiễu ngẫu nhiên (Pseudo-random noise)
            float hash(float2 n) { 
                return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453); 
            }

            // Hàm nhiễu giá trị (Value Noise) để tạo vân mây/bụi mượt mà
            float noise(float2 p) {
                float2 ip = floor(p);
                float2 u = frac(p);
                u = u * u * (3.0 - 2.0 * u);

                float res = lerp(
                    lerp(hash(ip), hash(ip + float2(1.0, 0.0)), u.x),
                    lerp(hash(ip + float2(0.0, 1.0)), hash(ip + float2(1.0, 1.0)), u.x), u.y);
                return res * res;
            }

            // Hàm FBM (Fractal Brownian Motion) để tạo chi tiết nhiều lớp
            float fbm(float2 p) {
                float f = 0.0;
                float m = 0.5;
                for (int i = 0; i < 3; i++) { // Lặp 3 lớp nhiễu
                    f += m * noise(p); 
                    p *= 2.0; 
                    m *= 0.5;
                }
                return f;
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Chuyển UV về hệ tọa độ cực (Polar Coordinates)
                // Gốc tọa độ đặt ở chính giữa cạnh trên (0.5, 1.0)
                float2 uv = i.uv;
                float2 pos = uv - float2(0.5, 1.0); 
                
                // r = khoảng cách từ đỉnh đèn (độ dài tia)
                // theta = góc của tia so với trục thẳng đứng
                float r = length(pos);
                float theta = atan2(pos.y, pos.x); // Góc trong khoảng -PI đến PI

                // 2. Tạo hình nón (Cone Mask)
                // Tính độ lệch của pixel so với trục giữa (0.5)
                float distFromCenter = abs(uv.x - 0.5);
                // Độ rộng chùm sáng tăng dần khi đi xuống (theo trục y)
                // (1.0 - uv.y) càng lớn (càng xuống dưới) thì chùm càng rộng
                float beamWidth = (1.0 - uv.y) * _SpotAngle; 
                
                // Smoothstep để làm mềm biên
                float coneMask = smoothstep(beamWidth, beamWidth - _Softness * (1.0-uv.y), distFromCenter);

                // 3. Hiệu ứng bụi/tia sáng (Volumetric Noise)
                // Chúng ta sample noise dựa trên tọa độ cực để các tia tỏa ra từ tâm
                float2 noiseUV = float2(theta * _RayStretch, r * _NoiseScale - _Time.y * _NoiseSpeed);
                float dust = fbm(noiseUV);

                // 4. Độ suy giảm ánh sáng (Vertical Falloff)
                // Sáng nhất ở đỉnh (uv.y = 1) và tối dần xuống dưới
                float falloff = pow(uv.y, _FadePower);

                // 5. Nguồn sáng điểm (Hotspot) ở ngay đỉnh đèn
                float hotspot = smoothstep(0.05, 0.0, r);

                // 6. Tổng hợp kết quả
                // Kết hợp hình nón, bụi và độ suy giảm
                float finalAlpha = coneMask * falloff * (0.5 + 0.5 * dust); 
                finalAlpha += hotspot; // Cộng thêm điểm sáng ở đỉnh
                
                // Áp dụng màu sắc và cường độ
                float3 finalColor = _MainColor.rgb * finalAlpha * _Intensity;

                return fixed4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}