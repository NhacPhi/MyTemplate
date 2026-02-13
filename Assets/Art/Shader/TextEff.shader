Shader "Custom/BuddhistWaveEffect"
{
    Properties
    {
        _MainTex ("Texture Hoa Van (Optional)", 2D) = "white" {}
        _Color ("Mau Vàng Phat Phap", Color) = (1.0, 0.75, 0.0, 1.0)
        _WaveSpeed ("Toc Do Song", Range(0.1, 5.0)) = 1.0
        _WaveStrength ("Do Manh Song", Range(0.01, 0.2)) = 0.05
        _WaveFrequency ("Tan So Song", Range(1.0, 20.0)) = 5.0
        _ScrollSpeed ("Toc Do Cuon", Vector) = (0.1, 0.2, 0, 0)
        _Glow ("Do Sang Phat Quang", Range(1.0, 5.0)) = 2.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One 
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

            sampler2D _MainTex;
            float4 _MainTex_ST; // Khai báo thêm để xử lý Tiling/Offset
            fixed4 _Color;
            float _WaveSpeed, _WaveStrength, _WaveFrequency, _Glow;
            float4 _ScrollSpeed;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Áp dụng Tiling và Offset mặc định của Unity
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;

                // 1. Tính toán biến dạng UV (Sóng lượn như vải)
                // Kết hợp Sin và Cos để tạo chuyển động hỗn hợp tự nhiên
                float uvOffset = sin(i.uv.x * _WaveFrequency + time * _WaveSpeed) * _WaveStrength;
                uvOffset += cos(i.uv.y * _WaveFrequency * 0.5 + time * _WaveSpeed) * _WaveStrength;

                float2 distortedUV = i.uv + uvOffset;

                // 2. Cuộn Texture theo thời gian
                distortedUV += _ScrollSpeed.xy * time;

                // 3. Lấy màu từ Texture
                fixed4 tex = tex2D(_MainTex, distortedUV);
                
                // 4. Kết hợp màu sắc và độ sáng
                // _Color.a giúp bạn chỉnh độ trong suốt tổng thể trong Inspector
                fixed4 col = tex * _Color * _Glow;

                // 5. Hiệu ứng mờ dần ở biên (Vignette) 
                // Giúp "tấm vải năng lượng" không bị cắt sắc cạnh
                float edgeFade = smoothstep(0.0, 0.15, i.uv.x) * smoothstep(1.0, 0.85, i.uv.x) *
                                 smoothstep(0.0, 0.15, i.uv.y) * smoothstep(1.0, 0.85, i.uv.y);
                
                col.a = tex.a * _Color.a * edgeFade;

                return col;
            }
            ENDCG
        }
    }
}