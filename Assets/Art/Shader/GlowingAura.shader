Shader "Custom/GlowingAura"
{
    Properties
    {
        [HDR] _AuraColor ("Aura Color", Color) = (1.0, 0.8, 0.2, 1.0) // Màu của hào quang
        _RectWidth ("Rectangle Width", Range(0.1, 1.0)) = 0.8         // Chiều rộng của hình chữ nhật
        _RectHeight ("Rectangle Height", Range(0.1, 1.0)) = 0.6       // Chiều cao của hình chữ nhật
        _CornerRadius ("Corner Radius", Range(0.0, 0.5)) = 0.1        // Độ bo góc của hình chữ nhật
        _GlowSpread ("Glow Spread (Softness)", Range(0.01, 1.0)) = 0.3// Độ tỏa của hào quang ra xung quanh
        _Thickness ("Outline Thickness", Range(0.0, 0.2)) = 0.02      // Độ dày của viền sáng (nếu muốn viền rõ)
        _Intensity ("Intensity", Range(0.0, 10.0)) = 3.0              // Cường độ phát sáng
        _PulseSpeed ("Pulse Speed", Range(0.0, 10.0)) = 3.0           // Tốc độ nhấp nháy/thở của ánh sáng
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "IgnoreProjector"="True" 
            "PreviewType"="Plane" 
        }
        LOD 100

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off // Vẽ cả 2 mặt

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _AuraColor;
                float _RectWidth;
                float _RectHeight;
                float _CornerRadius;
                float _GlowSpread;
                float _Thickness;
                half _Intensity;
                float _PulseSpeed;
            CBUFFER_END

            // Hàm tính toán khoảng cách đến hình chữ nhật bo góc (SDF - Signed Distance Field)
            float sdRoundedBox(float2 p, float2 b, float r)
            {
                float2 d = abs(p) - b + float2(r, r);
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - r;
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Đưa UV về gốc tọa độ ở giữa (-0.5 đến 0.5)
                float2 p = input.uv - 0.5;
                
                // Kích thước của hộp (chia 2 vì nó trải dài ra 2 phía từ tâm)
                float2 boxSize = float2(_RectWidth, _RectHeight) * 0.5;
                
                // Tính khoảng cách SDF từ điểm hiện tại tới viền của hình chữ nhật
                // sdf = 0 nằm đúng trên viền, sdf < 0 ở bên trong, sdf > 0 ở bên ngoài
                float sdf = sdRoundedBox(p, boxSize, _CornerRadius);
                
                // Muốn tạo viền sáng (Outline), ta lấy trị tuyệt đối của sdf
                // edgeDist = 0 là viền, càng xa viền thì số càng lớn
                float edgeDist = abs(sdf);
                
                // Tính toán độ mờ của hào quang: 
                // Ở gần viền (edgeDist nhỏ) thì sáng nhất (glow = 1)
                // Càng ra xa viền (edgeDist lớn dần) thì glow giảm về 0
                float glow = smoothstep(_GlowSpread, 0.0, edgeDist - _Thickness);
                
                // Hiệu ứng "Thở" / Nhấp nháy (Pulsing) sử dụng hàm Sin
                // Làm cho nó không bị tắt hẳn, dao động từ 0.6 đến 1.0
                float pulse = (sin(_Time.y * _PulseSpeed) + 1.0) * 0.5;
                pulse = lerp(0.6, 1.0, pulse); 
                
                // Kết hợp màu sắc
                half3 finalColor = _AuraColor.rgb * glow * pulse * _Intensity;
                half alpha = glow * pulse * _AuraColor.a;
                
                // Vùng bên trong hình chữ nhật (sdf < 0) nếu muốn rỗng thì không làm gì thêm
                // Vì edgeDist lấy abs(sdf) nên hào quang sẽ tỏa ra cả bên ngoài VÀ bên trong rìa.
                
                return half4(finalColor, saturate(alpha));
            }
            ENDHLSL
        }
    }
}
