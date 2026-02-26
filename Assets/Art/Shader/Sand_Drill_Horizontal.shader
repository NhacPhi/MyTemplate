Shader "Custom/AbstractWind_Step1_Sand"
{
    Properties
    {
        [Header(Orbit Settings)]
        [HDR] _Color ("Orbit Glow Color", Color) = (0.7, 0.9, 1.0, 1.0)
        _Intensity ("Orbit Intensity", Float) = 1.2
        _Speed ("Spin Speed", Float) = 1.0
        _Thickness ("Line Thickness", Range(0.001, 0.1)) = 0.015
        
        [Header(Flying Sand Settings)]
        [HDR] _SandColor ("Sand Color", Color) = (1.0, 0.8, 0.4, 1.0)
        _SandIntensity ("Sand Glow Intensity", Float) = 3.0 // Đã thêm: Giúp cát rực sáng
        _SandSpeedX ("Sand Speed X", Float) = 3.0
        _SandSpeedY ("Sand Speed Y", Float) = 1.0
        _SandDensity ("Sand Density", Float) = 30.0 // Đã giảm xuống để hạt to và dễ nhìn hơn
        _SandAmount ("Sand Amount threshold", Range(0.5, 1.0)) = 0.9 // Hạ xuống để cát xuất hiện nhiều hơn

        [Header(Masking)]
        _EdgeFade ("Edge Mask Softness", Range(0.1, 1.0)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Blend One One 
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color, _SandColor;
            float _Speed, _Thickness, _Intensity, _EdgeFade;
            float _SandIntensity, _SandSpeedX, _SandSpeedY, _SandDensity, _SandAmount;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float random(float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
            }

            float drawGlowingArc(float2 uv, float rotation, float radiusX, float radiusY, float thickness, float2 fadeDir) {
                float s = sin(rotation); float c = cos(rotation);
                float2x2 rotMat = float2x2(c, -s, s, c);
                float2 rotatedUV = mul(rotMat, uv);

                float dist = length(float2(rotatedUV.x / radiusX, rotatedUV.y / radiusY));
                float lineDistance = abs(dist - 1.0); 

                float core = smoothstep(thickness, 0.0, lineDistance);
                float halo = (thickness * 0.4) / (lineDistance + 0.002);
                
                float fade = smoothstep(-0.8, 0.8, dot(rotatedUV, normalize(fadeDir)));
                return (core + halo) * fade;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.uv * 2.0 - 1.0;
                float time = _Time.y;

                // --- 1. LỚP QUỸ ĐẠO NĂNG LƯỢNG ---
                float orbitTime = time * _Speed;
                float finalGlow = 0.0;
                finalGlow += drawGlowingArc(uv, 0.5 + orbitTime * 0.5, 0.8, 0.5, _Thickness, float2(0.0, 1.0));
                finalGlow += drawGlowingArc(uv, -0.2 + orbitTime * 0.7, 0.7, 0.6, _Thickness * 0.6, float2(1.0, 0.5));
                finalGlow += drawGlowingArc(uv, 1.2 - orbitTime * 0.4, 0.85, 0.35, _Thickness * 2.0, float2(-1.0, 0.2)) * 0.4;
                finalGlow += drawGlowingArc(uv, 2.0 + orbitTime * 0.8, 0.6, 0.3, _Thickness * 0.8, float2(0.5, -1.0));
                
                float3 orbitColorResult = _Color.rgb * finalGlow * _Intensity;

                // --- 2. LỚP CÁT BAY (ĐÃ ĐƯỢC CẢI TIẾN) ---
                // Chia lưới không gian, làm dẹp hạt cát thành vệt ngang
                float2 sandUV = uv * float2(_SandDensity, _SandDensity * 0.3); 
                sandUV.x += time * _SandSpeedX;
                sandUV.y += time * _SandSpeedY;

                float2 gridID = floor(sandUV);         // Lấy ID của từng ô lưới
                float2 localUV = frac(sandUV) - 0.5;   // Lấy tọa độ bên trong từng ô (từ -0.5 đến 0.5)

                float noiseVal = random(gridID);

                // Lọc số lượng cát (noiseVal càng lớn hơn _SandAmount thì càng ít cát)
                float showParticle = step(_SandAmount, noiseVal);

                // Vẽ hình dáng hạt cát: Sáng chói ở tâm và mờ dần ra viền (thay vì chấm vuông thô kệch)
                float particleShape = smoothstep(0.4, 0.0, length(localUV));

                // Kết hợp hiển thị và hình dáng
                float sandParticle = showParticle * particleShape;

                // Thêm hiệu ứng nhấp nháy lấp lánh ngẫu nhiên
                sandParticle *= (sin(time * 20.0 + noiseVal * 100.0) * 0.5 + 0.5);

                // Nhân với màu và cường độ sáng
                float3 sandColorResult = sandParticle * _SandColor.rgb * _SandIntensity;

                // --- 3. MASK BẢO VỆ VÀ TỔNG HỢP ---
                float edgeMask = smoothstep(1.0, 1.0 - _EdgeFade, length(uv));
                
                float3 finalRGB = (orbitColorResult + sandColorResult) * edgeMask;
                float finalAlpha = (finalGlow + sandParticle) * edgeMask;

                return float4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
}