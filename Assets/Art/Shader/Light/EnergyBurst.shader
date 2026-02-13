Shader "Custom/EnergyBurst_NoLoop_Fixed"
{
    Properties
    {
        _MainColor ("Màu Tâm", Color) = (1, 0.9, 0.2, 1)
        _RayColor ("Màu Tia Sáng", Color) = (1, 0.5, 0.0, 1)
        _BurstSpeed ("Tốc Độ Nổ", Range(0.1, 5.0)) = 2.0
        _RayDensity ("Mật Độ Tia", Range(5.0, 50.0)) = 30.0
        _Glow ("Độ Sáng", Range(1.0, 10.0)) = 4.0
        _StartTime ("Start Time", Float) = 999999 // Mặc định để rất lớn để ẩn lúc đầu
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One 
        ZWrite Off

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

            fixed4 _MainColor, _RayColor;
            float _BurstSpeed, _RayDensity, _Glow, _StartTime;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float timeSinceStart = _Time.y - _StartTime;

                // CHẶN HIỆU ỨNG: Nếu thời gian chưa tới hoặc đã nổ xong (quá 1 giây)
                // discard sẽ hủy pixel đó, giúp object tàng hình hoàn toàn
                if (timeSinceStart < 0 || timeSinceStart > 1.0 / _BurstSpeed) {
                    discard;
                }

                float2 centerUV = i.uv - 0.5;
                float dist = length(centerUV);
                float angle = atan2(centerUV.y, centerUV.x);

                float pulse = saturate(timeSinceStart * _BurstSpeed); 

                // 1. Tia sáng
                float rays = sin(angle * _RayDensity + timeSinceStart * _BurstSpeed * 5.0);
                rays = smoothstep(0.5, 1.0, rays);

                // 2. Vòng tròn nổ
                float ring = smoothstep(pulse - 0.1, pulse, dist) * smoothstep(pulse + 0.1, pulse, dist);

                // 3. Lõi tâm
                float core = smoothstep(0.2, 0.0, dist) * (1.0 - pulse);

                // 4. Kết hợp màu
                fixed4 finalCol = lerp(_RayColor, _MainColor, core + rays);
                float fade = (1.0 - pulse) * smoothstep(0.5, 0.0, dist);
                
                return finalCol * (rays + ring + core) * _Glow * fade;
            }
            ENDCG
        }
    }
}