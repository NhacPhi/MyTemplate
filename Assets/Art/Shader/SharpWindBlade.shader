Shader "Custom/WindStorm_Blade_2D"
{
    Properties
    {
        _MainColor ("Màu chính", Color) = (0.2, 1, 0.8, 1)
        _Speed ("Tốc độ xoay", Float) = 10.0
        _BladeSharpness ("Độ sắc lưỡi dao", Range(10, 300)) = 150
        _SpiralTightness ("Độ xoắn", Float) = 5.0
        _Density ("Mật độ tia", Range(0.1, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
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

            float4 _MainColor;
            float _Speed, _BladeSharpness, _SpiralTightness, _Density;

            float hash(float n) { return frac(sin(n) * 43758.5453123); }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - 0.5; // Đưa gốc tọa độ về tâm
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 1. Chuyển sang tọa độ cực (Polar Coordinates)
                float radius = length(i.uv);
                float angle = atan2(i.uv.y, i.uv.x);

                // 2. Tạo hiệu ứng xoắn ốc (Spiral)
                // Góc quay thay đổi dựa trên bán kính và thời gian
                float spiral = angle + (radius * _SpiralTightness) - (_Time.y * _Speed);

                // 3. Tạo các dải lưỡi dao dựa trên góc xoay
                // Chia vòng tròn thành nhiều phần bằng hàm sin
                float blades = sin(spiral * 8.0); 
                
                // Thêm một chút nhiễu ngẫu nhiên theo bán kính để các tia không đều nhau
                blades += hash(floor(radius * 20.0)) * 0.5;

                // 4. Áp dụng độ sắc lẹm
                // Chỉ giữ lại những đỉnh nhọn nhất của hàm sin
                float cut = saturate(blades - (1.0 - _Density));
                float finalBlade = pow(cut, _BladeSharpness * 0.1);

                // 5. Giới hạn hình dáng (Masking)
                // Tạo hình cái phễu (trên to dưới nhỏ hoặc ngược lại)
                float stormMask = smoothstep(0.5, 0.0, radius); // Giới hạn trong hình tròn
                stormMask *= smoothstep(0.0, 0.1, radius);   // Đục lỗ ở tâm

                // 6. Màu sắc và độ sáng
                float intensity = finalBlade * stormMask;
                
                return fixed4(_MainColor.rgb * intensity, intensity);
            }
            ENDCG
        }
    }
}