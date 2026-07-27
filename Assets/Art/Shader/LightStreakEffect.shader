Shader "Custom/LightStreakEffect"
{
    Properties
    {
        [Header(Editor Preview Mode)]
        [Toggle] _EditorPreview ("Xem Thử Trong Editor (Editor Preview Loop)", Float) = 1

        [Header(Color Settings)]
        [HDR] _MainColor ("Màu Core Aura (Main Color)", Color) = (1.0, 0.5, 0.1, 1.0)
        [HDR] _StreakColor ("Màu Chùm Sáng (Streak Color)", Color) = (1.0, 0.95, 0.5, 1.0)
        _Glow ("Độ Cường Độ Sáng (Glow Intensity)", Range(1.0, 10.0)) = 4.0

        [Header(Short Beam Length And Speed)]
        _BeamLength ("Độ Dài Đoạn Sáng (Segment Length)", Range(0.05, 0.5)) = 0.2
        _AscendSpeed ("Tốc Độ Bay (Speed)", Range(0.1, 10.0)) = 2.5

        [Header(S Curve Path Settings)]
        _SAmplitude ("Độ Rộng Uốn Chữ S (S-Amplitude)", Range(0.0, 0.45)) = 0.25
        _SFrequency ("Số Lượng Vòng Chữ S (S-Frequency)", Range(0.5, 5.0)) = 1.0

        [Header(Light Bundle And Internal Lines)]
        _BundleWidth ("Độ Dày Chùm Sáng (Bundle Width)", Range(0.02, 0.3)) = 0.1
        _LineCount ("Số Lượng Dải Sáng (Line Count)", Range(2.0, 40.0)) = 12.0

        [Header(Fade Edge Settings)]
        _BottomFade ("Độ Mờ Chân (Bottom Fade)", Range(0.0, 0.3)) = 0.05
        _TopFade ("Độ Mờ Đỉnh (Top Fade)", Range(0.7, 1.0)) = 0.95
        _SideFade ("Độ Mờ 2 Bên (Side Fade)", Range(0.01, 0.3)) = 0.05

        [Header(Single Run Controller)]
        _StartTime ("Start Time (Time.time)", Float) = 999999
        _Reverse ("Dao Chieu Movement (0=Up, 1=Down)", Float) = 0
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True"
        }

        Blend One One // Additive blend cho hiệu ứng năng lượng phát sáng
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
            };

            fixed4 _MainColor;
            fixed4 _StreakColor;
            float _Glow;
            float _BeamLength;
            float _AscendSpeed;
            float _SAmplitude;
            float _SFrequency;
            float _BundleWidth;
            float _LineCount;
            float _BottomFade;
            float _TopFade;
            float _SideFade;
            float _StartTime;
            float _Reverse;
            float _EditorPreview;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Tổng thời gian hiệu ứng chạy 1 lượt
                float totalDuration = (1.0 + _BeamLength) / max(_AscendSpeed, 0.01);

                float elapsedTime = _Time.y - _StartTime;

                // Nếu đang bật Editor Preview (Default = 1), lặp lại liên tục để xem trước trực tiếp trong Unity Editor
                if (_EditorPreview > 0.5)
                {
                    elapsedTime = fmod(_Time.y, totalDuration);
                }
                else
                {
                    // Chế độ Runtime Single-Run: Nếu chưa tới thời gian hoặc đã chạy xong 1 lượt -> Discard (tàng hình)
                    if (elapsedTime < 0.0 || elapsedTime > totalDuration)
                    {
                        discard;
                    }
                }

                float2 uv = i.uv;

                // Vị trí đầu chùm sáng & Khoảng cách từ Y tới đầu chùm sáng
                float headPos = 0.0;
                float distFromHead = 0.0;

                if (_Reverse > 0.5)
                {
                    // Chuyển động từ TRÊN xuống DƯỚI (Top -> Bottom)
                    headPos = (1.0 + _BeamLength) - elapsedTime * _AscendSpeed;
                    distFromHead = uv.y - headPos;
                }
                else
                {
                    // Chuyển động từ DƯỚI lên TRÊN (Bottom -> Top)
                    headPos = elapsedTime * _AscendSpeed;
                    distFromHead = headPos - uv.y;
                }

                // Giới hạn chùm sáng thành một ĐOẠN NGẮN (Short Segment)
                float segmentLengthMask = smoothstep(_BeamLength, 0.0, distFromHead) * smoothstep(-0.02, 0.02, distFromHead);

                // Đường dẫn chữ S cố định
                float sOffset = sin(uv.y * _SFrequency * 6.28318) * _SAmplitude;
                float sCenterX = 0.5 + sOffset;

                // Khoảng cách ngang tới tâm đường chữ S
                float distToCenter = abs(uv.x - sCenterX);

                // Lõi chùm sáng
                float coreBeam = smoothstep(_BundleWidth, 0.0, distToCenter);
                float outerGlow = smoothstep(_BundleWidth * 2.2, 0.0, distToCenter) * 0.4;

                // Các tia dải sáng cuộn xoắn bên trong đoạn ngắn
                float lineTwist = sin((distToCenter / (_BundleWidth + 0.001) * 3.14159) * _LineCount + elapsedTime * 10.0);
                float lineStreaks = pow(clamp(lineTwist * 0.5 + 0.5, 0.0, 1.0), 2.0) * coreBeam;

                // Kết hợp đoạn ngắn với chùm sáng chữ S
                float activeSegment = (coreBeam + lineStreaks * 1.5 + outerGlow) * segmentLengthMask;

                // Fade viền cạnh
                float verticalFade = smoothstep(0.0, _BottomFade, uv.y) * smoothstep(1.0, _TopFade, uv.y);
                float horizontalFade = smoothstep(0.0, _SideFade, uv.x) * smoothstep(1.0, 1.0 - _SideFade, uv.x);
                float edgeMask = verticalFade * horizontalFade;

                // Tính màu HDR
                fixed3 streakCol = _StreakColor.rgb * (lineStreaks * 2.0 + coreBeam) * _Glow;
                fixed3 mainCol = _MainColor.rgb * (outerGlow + coreBeam * 0.5) * _Glow;

                fixed3 finalRGB = (streakCol + mainCol) * activeSegment * edgeMask * i.color.rgb;
                float finalAlpha = saturate(activeSegment * edgeMask * i.color.a);

                return fixed4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
}
