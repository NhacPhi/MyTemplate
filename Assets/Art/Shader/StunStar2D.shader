Shader "Custom/ProceduralDizzySpheres"
{
    Properties
    {
        // Màu sắc chính của các quả cầu
        [HDR] _Color ("Sphere Color", Color) = (1, 0.9, 0, 1)
        
        // BIẾN CHỈNH SỐ LƯỢNG (Dùng Float để Slider mượt, code sẽ ép về Int)
        _SphereCount ("Sphere Count", Range(1, 16)) = 3
        
        // Tốc độ xoay (vòng/giây)
        _Speed ("Rotation Speed", Float) = 2.0
        
        // Kích thước Elip bẹt (X rộng, Y hẹp)
        _EllipseRadii ("Ellipse Radii (X, Y)", Vector) = (0.4, 0.12, 0, 0)
        
        // Kích thước của từng quả cầu
        _SphereSize ("Sphere Size", Range(0.01, 0.2)) = 0.05
        
        // Độ mờ viền (tạo hiệu ứng 3D giả)
        _Softness ("Softness/Glow", Range(0.001, 0.1)) = 0.02
    }
    SubShader
    {
        // Cài đặt Render cho hiệu ứng 2D trong suốt
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            // Bật Alpha Blending
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Khai báo các biến từ Properties
            float4 _Color;
            float _SphereCount;
            float _Speed;
            float4 _EllipseRadii;
            float _SphereSize;
            float _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // HÀM TOÁN HỌC: Vẽ quả cầu procedural (Circle SDF)
            float drawSphere(float2 p, float r)
            {
                float dist = length(p);
                return smoothstep(r + _Softness, r - _Softness, dist);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Chuyển đổi tọa độ UV (0->1) sang tọa độ tâm (-0.5 -> 0.5)
                float2 uv = i.uv - 0.5;

                float finalAlpha = 0.0;
                const float PI = 3.1415926535;
                float animatedAngle = _Time.y * _Speed;

                // Ép kiểu số lượng về số nguyên
                int count = (int)_SphereCount;

                // VÒNG LẶP VẼ TỪNG QUẢ CẦU
                for (int index = 0; index < 16; index++)
                {
                    if (index >= count) break;

                    float phase = ((float)index / (float)count) * 2.0 * PI;
                    float currentAngle = phase + animatedAngle;

                    float2 spherePos = float2(cos(currentAngle), sin(currentAngle)) * _EllipseRadii.xy;

                    // --- TẠO CHIỀU SÂU GIẢ (PERSPECTIVE FAKE) ---
                    float perspectiveFactor = (sin(currentAngle) + 1.0) / 2.0; 

                    // ĐÃ SỬA LỖI Ở ĐÂY: Dùng _SphereSize thay vì _StarSize
                    float dynamicSize = _SphereSize * lerp(1.0, 0.6, perspectiveFactor);
                    float dynamicBrightness = lerp(1.0, 0.4, perspectiveFactor);

                    // --- VẼ QUẢ CẦU ---
                    float2 localizedUV = uv - spherePos;
                    float sphereAlpha = drawSphere(localizedUV, dynamicSize);

                    // --- TỔNG HỢP ---
                    finalAlpha = max(finalAlpha, sphereAlpha * dynamicBrightness);
                }

                return fixed4(_Color.rgb, _Color.a * finalAlpha);
            }
            ENDCG
        }
    }
}