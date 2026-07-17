Shader "Custom/SerpentineDragon2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Dragon Texture (Texture Rồng)", 2D) = "white" {}
        _Color ("Main Tint (Màu nhuộm chính)", Color) = (1, 1, 1, 1)

        [Header(Body Wave Animation)]
        _WaveSpeed ("Wave Speed (Tốc độ uốn)", Float) = 5.0
        _WaveFreq ("Wave Frequency (Tần số sóng/Số khúc)", Float) = 3.0
        _WaveAmp ("Wave Amplitude (Biên độ uốn)", Float) = 0.15
        
        _HeadPosition ("Head Position (Vị trí đầu 0-Trái, 1-Phải)", Range(0, 1)) = 1.0
        _HeadStability ("Head Stability (Độ tĩnh đầu rồng 0-1)", Range(0, 1)) = 0.8

        [Header(Breath Pulse)]
        _PulseSpeed ("Pulse Speed (Tốc độ thở)", Float) = 2.5
        _PulseAmp ("Pulse Amplitude (Độ phình cơ thể)", Float) = 0.03

        [Header(Magical Energy Flow)]
        _EnergyColor ("Energy Glow Color (Màu hào quang rồng)", Color) = (0.0, 0.8, 1.0, 1.0)
        _EnergySpeed ("Energy Scroll Speed (Tốc độ cuộn năng lượng)", Vector) = (1.2, 0.2, 0, 0)
        _EnergyScale ("Energy Noise Scale", Float) = 5.0
        _EnergyGlow ("Energy Glow Intensity", Range(0, 4)) = 1.5
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha // standard transparency
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 noiseUV : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            
            float _WaveSpeed;
            float _WaveFreq;
            float _WaveAmp;
            float _HeadPosition;
            float _HeadStability;
            
            float _PulseSpeed;
            float _PulseAmp;
            
            float4 _EnergyColor;
            float4 _EnergySpeed;
            float _EnergyScale;
            float _EnergyGlow;

            // Simple noise for energy flow
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash(i + float2(0.0, 0.0));
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;

                // 1. Serpentine Wave Animation (Uốn lượn hình chữ S)
                // Tính khoảng cách từ đỉnh hiện tại tới đầu rồng (để giảm rung ở phần đầu)
                float distToHead = abs(v.uv.x - _HeadPosition);
                
                // Trọng số giảm chấn: đầu rồng càng xa thì càng uốn nhiều, đầu rồng đứng yên
                // Nếu _HeadStability = 1, đầu rồng đứng yên hoàn toàn. Nếu = 0, đầu rồng lắc cùng biên độ
                float damping = lerp(1.0, distToHead, _HeadStability);
                
                // Sóng sine dựa trên thời gian và tọa độ trục dọc/ngang cơ thể
                float waveOffset = sin(_Time.y * _WaveSpeed + v.uv.x * _WaveFreq * 6.283) * _WaveAmp * damping;
                
                // 2. Pulse Animation (Hiệu ứng thở phập phồng)
                // Tác động co giãn nhẹ theo trục Y (độ rộng thân rồng) dựa trên nhịp thở
                float pulseOffset = cos(_Time.y * _PulseSpeed + v.uv.x * 3.14) * _PulseAmp;
                
                // Áp dụng biến đổi vị trí cục bộ (local vertex position)
                float4 localPos = v.vertex;
                localPos.y += waveOffset;
                
                // Co giãn trục Y nhẹ dựa trên nhịp thở (hướng từ tâm Y đi ra)
                float offsetYFromCenter = v.uv.y - 0.5;
                localPos.y += offsetYFromCenter * pulseOffset;

                o.vertex = UnityObjectToClipPos(localPos);

                // 3. Chuẩn bị UV dịch chuyển cho dòng chảy năng lượng trong fragment shader
                o.noiseUV = v.uv * _EnergyScale + _Time.y * _EnergySpeed.xy;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Lấy màu gốc của Texture Rồng
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                // Nếu vùng texture trong suốt (alpha = 0), bỏ qua để tối ưu
                clip(texColor.a - 0.001);

                // Tính toán dòng chảy năng lượng ánh sáng chạy dọc thân rồng
                float flowNoise = noise(i.noiseUV);
                float pulseGlow = sin(_Time.y * 3.0 + i.uv.x * 5.0) * 0.5 + 0.5;
                
                // Màu năng lượng rồng phát sáng
                float3 glowEffect = _EnergyColor.rgb * (flowNoise + pulseGlow * 0.3) * _EnergyGlow * texColor.a;
                
                // Trộn màu rồng gốc với màu năng lượng
                // Hào quang năng lượng sẽ phát sáng mạnh ở các vùng sáng của texture rồng
                float3 finalRGB = texColor.rgb * i.color.rgb + glowEffect * texColor.r; 
                
                return fixed4(finalRGB, texColor.a * i.color.a);
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
