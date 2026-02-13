Shader "Custom/BuddhistShockwave_NoLoop"
{
    Properties
    {
        _MainColor ("Màu Sóng (Vàng Kim)", Color) = (1, 0.9, 0.2, 1)
        _BurstSpeed ("Tốc Độ Lan Tỏa", Range(0.1, 5.0)) = 1.5
        _ShockwaveCount ("Số Vòng Sóng", Range(1, 5)) = 2
        _Thickness ("Độ Mảnh Của Vòng", Range(0.01, 0.1)) = 0.03
        _Glow ("Độ Sáng Rực", Range(1.0, 10.0)) = 5.0
        _StartTime ("Start Time", Float) = 999999
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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

            fixed4 _MainColor;
            float _BurstSpeed, _ShockwaveCount, _Thickness, _Glow, _StartTime;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float timeSinceStart = _Time.y - _StartTime;
                
                // CHẶN HIỆU ỨNG: Nếu chưa tới lúc nổ hoặc đã nổ xong chu kỳ của các vòng sóng
                // (Thêm 0.5s trừ hao để vòng cuối kịp biến mất hoàn toàn)
                if (timeSinceStart < 0 || timeSinceStart > (1.0 + 0.5) / _BurstSpeed) {
                    discard;
                }

                float2 centerUV = i.uv - 0.5;
                float dist = length(centerUV);
                float finalWave = 0;

                for (int j = 0; j < (int)_ShockwaveCount; j++) {
                    // Thay vì frac, ta dùng thời gian tuyến tính cộng với độ lệch (offset)
                    float pulse = (timeSinceStart * _BurstSpeed) - ((float)j * 0.2); 
                    
                    // Chỉ vẽ khi pulse nằm trong khoảng 0-1
                    if(pulse > 0 && pulse < 1.0) {
                        float wave = smoothstep(pulse - _Thickness, pulse, dist) * smoothstep(pulse + _Thickness, pulse, dist);
                        float fade = pow(1.0 - pulse, 2.0);
                        finalWave += wave * fade;
                    }
                }

                // Lõi sáng trung tâm cũng chạy theo thời gian thực
                float coreLife = saturate(timeSinceStart * _BurstSpeed * 2.0);
                float core = smoothstep(0.1, 0.0, dist) * (1.0 - coreLife);

                fixed4 col = _MainColor * (finalWave + core) * _Glow;
                float edgeMask = smoothstep(0.5, 0.4, dist);
                
                return col * edgeMask;
            }
            ENDCG
        }
    }
}