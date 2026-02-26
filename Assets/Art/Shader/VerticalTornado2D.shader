Shader "Custom/Tornado_Final_Wobble"
{
    Properties
    {
        [Header(Main Tornado Lines)]
        [HDR] _Color ("Neon Color", Color) = (0.0, 0.6, 1.0, 1.0)
        _Speed ("Spin Speed", Float) = 2.0
        _Density ("Line Density", Float) = 8.0
        _Width ("Tornado Width", Float) = 0.4
        _Tightness ("Twist Tightness", Float) = 12.0

        [Header(Overall Wobble Settings)]
        _WobbleAmp ("Wobble Amplitude", Float) = 0.05 // Độ rộng của cú lắc
        _WobbleFreq ("Wobble Frequency", Float) = 2.0  // Số lượng khúc uốn
        _WobbleSpeed ("Wobble Speed", Float) = 1.5    // Tốc độ đung đưa

        [Header(Bottom Dust Effect)]
        [HDR] _DustColor ("Dust Color", Color) = (0.8, 0.7, 0.5, 1.0)
        _DustHeight ("Dust Height Limit", Range(0.1, 1.0)) = 0.4
        _DustDensity ("Dust Density", Float) = 1.5
        _DustSpeed ("Dust Swirl Speed", Float) = 1.0
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

            float4 _Color, _DustColor;
            float _Speed, _Density, _Width, _Tightness;
            float _WobbleAmp, _WobbleFreq, _WobbleSpeed;
            float _DustHeight, _DustDensity, _DustSpeed;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // --- HÀM NOISE & FBM (GIỮ NGUYÊN) ---
            float random (float2 st) { return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123); }
            float noise (float2 st) {
                float2 i = floor(st); float2 f = frac(st);
                float a = random(i); float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0)); float d = random(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a)* u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }
            float fbm (float2 st) {
                float value = 0.0; float amp = 0.5;
                for (int i = 0; i < 3; i++) { value += amp * noise(st); st *= 2.0; amp *= 0.5; }
                return value;
            }

            float spiralLine(float2 uv, float offset) {
                float y = uv.y + 0.5;
                float width = y * _Width;
                float xPos = sin(_Time.y * _Speed + y * _Tightness + offset) * width;
                float dist = abs(uv.x - xPos);
                return pow(0.01 / (dist + 0.005), 1.5);
            }

            fixed4 frag (v2f i) : SV_Target {
                // --- BƯỚC QUAN TRỌNG: TẠO ĐỘ UỐN ÉO TỔNG THỂ ---
                // Dùng hàm Sin dựa trên tọa độ Y và thời gian để làm lệch trục X
                float wobble = sin(i.uv.y * _WobbleFreq + _Time.y * _WobbleSpeed) * _WobbleAmp;
                i.uv.x += wobble; 

                float2 uvCenter = i.uv - 0.5;
                
                // 1. Tính toán các đường xoắn (đã bị uốn theo wobble)
                float finalLines = 0;
                for(int j=0; j<5; j++) { finalLines += spiralLine(uvCenter, j * 1.25); }
                float lineMask = smoothstep(0.0, 0.2, i.uv.y) * smoothstep(1.0, 0.7, i.uv.y);
                float sparkleNoise = frac(sin(dot(i.uv, float2(12.9898, 78.233))) * 43758.5453);
                finalLines += finalLines * sparkleNoise * 0.1;
                float3 lineColorResult = finalLines * _Color.rgb * lineMask;

                // 2. Tính toán khói bụi (cũng bị uốn theo wobble)
                float2 dustUV = i.uv * float2(3.0, 2.0);
                dustUV.y -= _Time.y * _DustSpeed * 0.5;
                dustUV.x += sin(_Time.y * _DustSpeed) * 0.2;
                float dustClouds = fbm(dustUV);
                float dustVerticalMask = smoothstep(_DustHeight, 0.0, i.uv.y);
                float coneWidth = i.uv.y * _Width + 0.15;
                float dustHorizontalMask = smoothstep(coneWidth + 0.2, coneWidth, abs(uvCenter.x));
                float3 dustColorResult = dustClouds * dustVerticalMask * dustHorizontalMask * _DustDensity * _DustColor.rgb;

                return float4(lineColorResult + dustColorResult, 1.0);
            }
            ENDCG
        }
    }
}