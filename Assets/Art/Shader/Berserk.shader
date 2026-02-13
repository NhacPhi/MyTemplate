Shader "Custom/RageFlameVFX"
{
    Properties
    {
        _MainColor ("Màu Lửa Chính", Color) = (1, 0.4, 0, 1)
        _EdgeColor ("Màu Viền Lửa", Color) = (1, 0.1, 0, 1)
        _Speed ("Tốc Độ Cháy", Range(0.1, 5)) = 2.0
        _Density ("Mật Độ Ngọn Lửa", Range(1, 20)) = 8.0
        _Glow ("Độ Sáng rực", Range(1, 10)) = 5.0
        _VerticalFade ("Độ Cao Ngọn Lửa", Range(0.1, 2.0)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One // Cộng sáng rực rỡ
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

            fixed4 _MainColor, _EdgeColor;
            float _Speed, _Density, _Glow, _VerticalFade;

            // Hàm tạo Noise ngẫu nhiên
            float hash(float2 p) {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float noise(float2 p) {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i + float2(0,0)), hash(i + float2(1,0)), f.x),
                            lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), f.x), f.y);
            }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _Speed;
                
                // 1. Tạo chuyển động bốc lên cho UV
                float2 movingUV = i.uv * _Density;
                movingUV.y -= time; // Lửa chạy lên trên

                // 2. Kết hợp nhiều lớp Noise để tạo hình ngọn lửa
                float n = noise(movingUV);
                n += noise(movingUV * 2.1) * 0.5;
                n += noise(movingUV * 4.4) * 0.25;
                n /= 1.75;

                // 3. Tạo mặt nạ hình ngọn lửa (mỏng ở dưới, nhọn ở trên)
                // Càng lên cao (uv.y tăng) thì càng mờ dần
                float fade = pow(1.0 - i.uv.y, _VerticalFade);
                
                // Cắt bớt hai bên cạnh để lửa không bị vuông quá
                float sideFade = smoothstep(0.0, 0.2, i.uv.x) * smoothstep(1.0, 0.8, i.uv.x);
                
                float finalMask = n * fade * sideFade;

                // 4. Phối màu theo độ mạnh của n
                fixed4 col = lerp(_EdgeColor, _MainColor, finalMask);
                
                // Độ rực rỡ
                return col * finalMask * _Glow;
            }
            ENDCG
        }
    }
}