Shader "Custom/ProceduralGlintSparkle"
{
    Properties
    {
        [HDR] _Color ("Glow Color", Color) = (1.0, 0.9, 0.4, 1.0)
        _Speed ("Blink Speed", Range(0.1, 10)) = 3.0
        _RotationSpeed ("Rotation Speed", Range(-10, 10)) = 1.5
        _CoreSize ("Core Size", Range(0.1, 5)) = 1.5
        _BeamWidth ("Beam Thickness", Range(10, 100)) = 40.0
        _BeamLength ("Beam Length", Range(0.5, 5)) = 2.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha One   // Additive blending (chế độ phát sáng chói)
        ZWrite Off
        Cull Off

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
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            float4 _Color;
            float _Speed;
            float _RotationSpeed;
            float _CoreSize;
            float _BeamWidth;
            float _BeamLength;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Xoay tia sáng tự động theo thời gian
                float2 uv = v.uv - 0.5;
                float angle = _Time.y * _RotationSpeed;
                float s = sin(angle);
                float c = cos(angle);
                float2x2 rot = float2x2(c, -s, s, c);
                uv = mul(rot, uv);
                o.uv = uv + 0.5;
                
                o.color = v.color;
                return o;
            }

            // Hàm tạo hình ngôi sao lấp lánh (không cần dùng ảnh)
            float ProceduralStar(float2 uv)
            {
                float2 centered = uv - 0.5;
                
                // Lõi sáng ở giữa (Core glow)
                float dist = length(centered);
                float core = max(0.0, 1.0 - (dist * _CoreSize * 2.0));
                // Làm lõi cong mượt hơn
                core = pow(core, 2.0); 

                // Các tia sáng dài (Cross Beams)
                float beamX = max(0.0, 1.0 - abs(centered.y * _BeamWidth) - abs(centered.x * _BeamLength));
                float beamY = max(0.0, 1.0 - abs(centered.x * _BeamWidth) - abs(centered.y * _BeamLength));
                
                // Tia chéo phụ (nhỏ hơn tia chính)
                float2 diagonalUV = float2(
                    (centered.x + centered.y) * 0.707, 
                    (centered.x - centered.y) * 0.707
                );
                float diagX = max(0.0, 1.0 - abs(diagonalUV.y * (_BeamWidth * 1.5)) - abs(diagonalUV.x * (_BeamLength * 1.5)));
                float diagY = max(0.0, 1.0 - abs(diagonalUV.x * (_BeamWidth * 1.5)) - abs(diagonalUV.y * (_BeamLength * 1.5)));

                // Kết hợp tất cả lại
                return core * 0.8 + beamX + beamY + (diagX + diagY) * 0.5;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Tạo hình dáng tia sáng
                float starIntensity = ProceduralStar(i.uv);
                
                // Nhấp nháy to nhỏ / sáng tối
                float blink = (sin(_Time.y * _Speed) * 0.5) + 0.5;
                // Không bao giờ tắt hẳn, mờ nhất là 30%
                blink = lerp(0.3, 1.0, blink);

                // Tổng hợp cường độ
                float finalIntensity = starIntensity * blink;
                
                // Trả về màu phát sáng (pha trộn với màu tint của SpriteRenderer nếu có)
                return _Color * finalIntensity * i.color;
            }
            ENDCG
        }
    }
}
