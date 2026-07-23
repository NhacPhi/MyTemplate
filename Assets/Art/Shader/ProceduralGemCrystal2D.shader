Shader "Custom/ProceduralGemCrystal2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Alpha Mask (Hình dáng pha lê)", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Alpha ("Alpha (Độ trong suốt)", Range(0, 1)) = 0.6
        
        [Header(Crystal Colors)]
        [HDR] _TopColor ("Top Color (Màu Đỉnh Tinh Thể)", Color) = (0.8, 1.0, 1.0, 1.0)
        [HDR] _BottomColor ("Bottom Color (Màu Đáy)", Color) = (0.0, 0.3, 0.8, 1.0)
        [HDR] _EdgeColor ("Edge Highlight (Màu Viền Cắt)", Color) = (0.5, 0.9, 1.0, 1.0)
        
        [Header(Facets Settings)]
        _ScaleX ("Facet Scale X (Mật độ bề ngang)", Float) = 3.0
        _ScaleY ("Facet Scale Y (Mật độ bề dọc)", Float) = 6.0
        _EdgeThickness ("Edge Thickness (Độ dày nét cắt)", Range(0.001, 0.1)) = 0.03
        _Contrast ("Facet Contrast (Độ tương phản các mặt)", Range(0, 1)) = 0.4
        
        [Header(Lighting and Glow)]
        _ShineIntensity ("Top Glow (Độ rực sáng đỉnh)", Range(0, 3)) = 1.2
        _InnerGlow ("Inner Glow (Sáng từ tâm)", Range(0, 2)) = 0.5
        
        [Header(Mirror Shine Effect)]
        [HDR] _MirrorShineColor ("Shine Color (Màu Vệt Sáng)", Color) = (1.0, 1.0, 1.0, 1.0)
        _MirrorShineSpeed ("Shine Speed (Tốc độ quét)", Float) = 0.8
        _MirrorShineWidth ("Shine Width (Độ rộng vệt sáng)", Range(0.01, 0.5)) = 0.15
        _MirrorShineAngle ("Shine Angle (Góc nghiêng của vệt)", Range(-2, 2)) = 1.0

        [Header(Ice Mist Effect)]
        _MistColor ("Mist Color (Màu Khói Băng)", Color) = (0.6, 0.85, 1.0, 1.0)
        _MistSpeed ("Mist Speed (Tốc độ khói bay)", Float) = 0.5
        _MistDensity ("Mist Density (Độ đặc của khói)", Range(0, 3)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Alpha;
            
            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed4 _EdgeColor;
            
            float _ScaleX;
            float _ScaleY;
            float _EdgeThickness;
            float _Contrast;
            
            float _ShineIntensity;
            float _InnerGlow;

            fixed4 _MirrorShineColor;
            float _MirrorShineSpeed;
            float _MirrorShineWidth;
            float _MirrorShineAngle;

            fixed4 _MistColor;
            float _MistSpeed;
            float _MistDensity;

            // Hàm Random sinh số giả ngẫu nhiên
            float2 random2(float2 p) 
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453); 
            }

            // Thuật toán Voronoi tạo các mặt cắt pha lê (Facets)
            void voronoiFacet(float2 uv, out float cellBrightness, out float edgeDist) 
            {
                float2 n = floor(uv);
                float2 f = frac(uv);
                
                float2 mg, mr;
                float md = 8.0;
                
                // Pass 1: Tìm tâm của mặt cắt gần nhất
                for(int j=-1; j<=1; j++)
                {
                    for(int i=-1; i<=1; i++) 
                    {
                        float2 g = float2(float(i), float(j));
                        float2 o = random2(n + g);
                        float2 r = g + o - f;
                        float d = dot(r, r);
                        if(d < md) 
                        {
                            md = d;
                            mr = r;
                            mg = g;
                        }
                    }
                }
                
                // Tạo độ sáng ngẫu nhiên cho từng mặt cắt dựa vào ID của mặt đó
                cellBrightness = frac(sin(dot(n + mg, float2(12.9898, 78.233))) * 43758.5453);
                
                // Pass 2: Tính khoảng cách tới viền của mặt cắt để tạo nét cắt sắc cạnh
                md = 8.0;
                for(int j=-2; j<=2; j++)
                {
                    for(int i=-2; i<=2; i++) 
                    {
                        float2 g = mg + float2(float(i), float(j));
                        float2 o = random2(n + g);
                        float2 r = g + o - f;
                        if(dot(mr - r, mr - r) > 0.00001) 
                        {
                            float d = dot(0.5*(mr + r), normalize(r - mr));
                            md = min(md, d);
                        }
                    }
                }
                edgeDist = md;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1. Khung hình gốc (Sử dụng alpha của Sprite để cắt hình dáng)
                fixed4 mainTex = tex2D(_MainTex, i.texcoord);
                clip(mainTex.a - 0.01);
                float finalAlpha = mainTex.a * _Alpha;

                // 2. Kéo dãn tọa độ UV để tạo các mặt cắt pha lê dài (giống viên ngọc)
                float2 crystalUV = i.texcoord * float2(_ScaleX, _ScaleY);
                
                float cellBrightness, edgeDist;
                voronoiFacet(crystalUV, cellBrightness, edgeDist);
                
                // 3. Gradient màu nền (Từ dưới lên trên)
                float verticalGrad = i.texcoord.y; // 0 ở đáy, 1 ở đỉnh
                fixed3 gradientColor = lerp(_BottomColor.rgb, _TopColor.rgb, verticalGrad);
                
                // 4. Áp dụng độ sáng ngẫu nhiên cho từng mặt cắt (Facet shading)
                // Các mặt sẽ có độ tối/sáng khác nhau tạo cảm giác góc cạnh 3D
                float faceModifier = lerp(1.0 - _Contrast, 1.0 + _Contrast, cellBrightness);
                fixed3 faceColor = gradientColor * faceModifier;
                
                // 5. Viền cắt sắc nét (Edge lines)
                float edgeLines = smoothstep(_EdgeThickness, 0.0, edgeDist);
                faceColor += _EdgeColor.rgb * edgeLines * (0.5 + verticalGrad * 0.5); // Viền ở đỉnh sáng hơn ở đáy
                
                // 6. Ánh sáng chiếu chói lóa ở đỉnh (Top Glow)
                float topShine = smoothstep(0.6, 1.0, verticalGrad);
                faceColor += _TopColor.rgb * topShine * _ShineIntensity;
                
                // 7. Ánh sáng rực lên từ tâm khối pha lê (Inner Glow)
                float2 dirFromCenter = i.texcoord - float2(0.5, 0.4); // Tâm hơi lệch xuống dưới một chút
                float distFromCenter = dot(dirFromCenter, dirFromCenter);
                float innerGlow = smoothstep(0.15, 0.0, distFromCenter);
                faceColor += _TopColor.rgb * innerGlow * _InnerGlow;

                // 8. Hiệu ứng Khói Băng (Ice Mist) - Khói lạnh bay lượn sóng bên trong pha lê
                float2 mistUV = i.texcoord * 4.0; // Tỉ lệ khói
                mistUV.y -= _Time.y * _MistSpeed; // Khói bay dâng lên trên
                mistUV.x += sin(mistUV.y * 2.0 + _Time.y) * 0.3; // Độ cuộn lượn sóng
                // Tạo độ nhiễu sóng (Noise) đơn giản cho khói
                float mistNoise = (sin(mistUV.x * 3.14) + cos(mistUV.y * 2.7)) * 0.25 + 0.5;
                float mistMask = smoothstep(0.4, 0.8, mistNoise) * _MistDensity;
                // Khói chỉ xuất hiện rõ ở nửa dưới của tinh thể và tan dần khi lên đỉnh
                float mistFade = smoothstep(0.8, 0.1, i.texcoord.y);
                faceColor += _MistColor.rgb * mistMask * mistFade * _MistColor.a;

                // 9. Hiệu ứng Sáng Gương (Mirror Shine Sweep) - Vệt chớp lóe quét ngang qua
                // Chỉnh góc quét
                float sweepPos = i.texcoord.x * _MirrorShineAngle + i.texcoord.y;
                // Thời gian lặp lại của vệt sáng
                float sweepTime = frac(_Time.y * _MirrorShineSpeed);
                // Bù trừ khoảng cách để vệt quét bao phủ toàn bộ tinh thể từ ngoài vào trong
                float distToSweep = abs(sweepPos - (sweepTime * 4.0 - 1.0));
                
                // Lớp vệt sáng mờ bao quanh
                float mirrorShine = smoothstep(_MirrorShineWidth, 0.0, distToSweep);
                // Lõi vệt sáng cực chói ở giữa
                float coreShine = smoothstep(_MirrorShineWidth * 0.2, 0.0, distToSweep);
                
                faceColor += _MirrorShineColor.rgb * (mirrorShine * 0.4 + coreShine * 2.0);

                return fixed4(faceColor, finalAlpha);
            }
            ENDCG
        }
    }
}
