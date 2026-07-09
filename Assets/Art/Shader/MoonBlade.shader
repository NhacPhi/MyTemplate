Shader "Custom/MoonBlade"
{
    Properties
    {
        [HDR] _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        [HDR] _GlowColor ("Outer Glow Color", Color) = (0.2, 0.6, 1.0, 1)
        
        _Curvature ("Blade Curvature", Range(0.0, 5.0)) = 2.0
        _OffsetX ("Horizontal Offset", Range(-1.0, 1.0)) = 0.5
        
        _Thickness ("Glow Thickness (Tail)", Range(0.1, 2.0)) = 0.6
        _CoreThickness ("Core Thickness", Range(0.01, 0.5)) = 0.1
        _Sharpness ("Leading Edge Sharpness", Range(0.001, 0.5)) = 0.05
        
        _FadeTips ("Tip Sharpness", Range(0.0, 1.0)) = 0.5
        
        _Speed ("Energy Flow Speed", Range(0.0, 50.0)) = 15.0
        _EnergyDensity ("Energy Flow Density", Range(0.0, 50.0)) = 20.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        // Blend Additive: Giúp lưỡi kiếm phát sáng rực rỡ khi đè lên cảnh vật
        Blend SrcAlpha One
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _CoreColor;
            float4 _GlowColor;
            float _Curvature;
            float _OffsetX;
            float _Thickness;
            float _CoreThickness;
            float _Sharpness;
            float _FadeTips;
            float _Speed;
            float _EnergyDensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Chuyển tọa độ UV từ [0, 1] sang [-1, 1] để dễ tính toán đối xứng
                float2 uv = i.uv * 2.0 - 1.0;

                // Tính toán hình dáng Parabol (hình vòng cung của mặt trăng)
                // Lưỡi kiếm sẽ cong theo trục Y và hướng về trục -X
                float targetX = -uv.y * uv.y * _Curvature + _OffsetX;
                
                // Khoảng cách từ vị trí hiện tại tới sống kiếm
                float dist = uv.x - targetX;

                // 1. Cạnh sắc (Leading Edge): Cắt gắt ở phía trước
                float front = smoothstep(_Sharpness, 0.0, dist);

                // 2. Phần đuôi mờ (Trailing Edge): Mờ dần về phía sau tạo cảm giác vệt chém
                float backGlow = smoothstep(-_Thickness, 0.0, dist);
                float backCore = smoothstep(-_CoreThickness, 0.0, dist);

                // 3. Chuốt nhọn 2 đầu kiếm (Tips)
                // UV.y = 1 hoặc -1 là 2 đỉnh. Càng gần 1 thì fade out càng nhanh
                float tips = smoothstep(1.0, 1.0 - _FadeTips, abs(uv.y));

                // Tính toán lớp Lõi sáng (Core) và Hào quang (Glow)
                float coreMask = front * backCore * tips;
                float glowMask = front * backGlow * tips;

                // 4. Hiệu ứng năng lượng (Energy Flow) chạy dọc theo lưỡi kiếm
                float energyFlow = sin(uv.y * _EnergyDensity - _Time.y * _Speed) * 0.5 + 0.5;
                // Làm cho hào quang lấp lánh và có vân năng lượng
                glowMask *= (0.7 + energyFlow * 0.3);

                // 5. Kết hợp màu sắc
                float4 col = _CoreColor * coreMask + _GlowColor * glowMask * (1.0 - coreMask);
                
                // Kênh Alpha
                col.a = max(coreMask, glowMask);

                return col;
            }
            ENDCG
        }
    }
}
