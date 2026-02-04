Shader "Custom/HealingCrossParticles"
{
    Properties
    {
        _Color ("Cross Color", Color) = (0.2, 1.0, 0.6, 1.0)
        _Size ("Cross Size", Range(0.01, 0.2)) = 0.08
        _Speed ("Rising Speed", Range(0.1, 2.0)) = 0.5
        _Density ("Density", Range(1, 10)) = 5.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One // Chế độ Additive giúp các dấu thập phát sáng
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

            fixed4 _Color;
            float _Size;
            float _Speed;
            float _Density;

            // Hàm vẽ hình chữ thập
            float drawCross(float2 uv, float2 center, float size) {
                float2 d = abs(uv - center);
                // Thanh dọc và thanh ngang
                float vertical = smoothstep(size, size - 0.01, d.x) * smoothstep(size * 3.0, size * 3.0 - 0.01, d.y);
                float horizontal = smoothstep(size * 3.0, size * 3.0 - 0.01, d.x) * smoothstep(size, size - 0.01, d.y);
                return saturate(vertical + horizontal);
            }

            // Hàm tạo số ngẫu nhiên
            float rand(float n) { return frac(sin(n) * 43758.5453123); }

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float finalMask = 0.0;
                
                // Tạo nhiều hình chữ thập dựa trên _Density
                for(float j = 0; j < _Density; j++) {
                    // Mỗi hình chữ thập có thời gian lệch nhau (offset)
                    float offset = rand(j);
                    float t = frac(_Time.y * _Speed + offset); // Chạy từ 0 đến 1
                    
                    // Vị trí: X ngẫu nhiên, Y bay từ dưới (0) lên trên (1)
                    float2 pos = float2(rand(j + 10.0), t);
                    
                    // Hiệu ứng: To dần ở giữa và mờ dần khi lên cao
                    float currentSize = _Size * smoothstep(0.0, 0.5, t);
                    float fade = smoothstep(1.0, 0.7, t) * smoothstep(0.0, 0.2, t);
                    
                    finalMask += drawCross(i.uv, pos, currentSize) * fade;
                }

                fixed4 col = _Color * finalMask;
                return col;
            }
            ENDCG
        }
    }
}