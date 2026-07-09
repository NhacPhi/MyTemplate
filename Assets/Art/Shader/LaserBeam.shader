Shader "Custom/LaserBeam"
{
    Properties
    {
        [HDR] _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        [HDR] _OuterColor ("Outer Glow Color", Color) = (0, 0.8, 1, 1)
        
        _CoreThickness ("Core Thickness", Range(0.01, 1.0)) = 0.2
        _OuterThickness ("Outer Glow Thickness", Range(0.1, 2.0)) = 0.8
        
        _Speed ("Ripple Speed", Range(0.0, 50.0)) = 20.0
        _RippleDensity ("Ripple Density", Range(0.0, 50.0)) = 30.0
        
        _PulseSpeed ("Pulse Speed", Range(0.0, 20.0)) = 10.0
        _PulseIntensity ("Pulse Intensity", Range(0.0, 1.0)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        // Chế độ Blend Additive (sáng lên khi đè nhau) phù hợp cho Laser
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
            float4 _OuterColor;
            float _CoreThickness;
            float _OuterThickness;
            float _Speed;
            float _RippleDensity;
            float _PulseSpeed;
            float _PulseIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Tính khoảng cách từ tâm của trục Y (tâm = 0.5)
                float distFromCenter = abs(i.uv.y - 0.5) * 2.0;

                // Hiệu ứng "nhịp đập" tổng thể theo thời gian
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseIntensity;
                
                // Hiệu ứng gợn sóng chạy dọc theo trục X của laser
                float ripple = sin(i.uv.x * _RippleDensity - _Time.y * _Speed);
                float rippleModifier = 1.0 + ripple * 0.15; // Tăng giảm độ dày một chút

                // Tính toán lõi sáng bên trong (sáng chói và sắc nét)
                float core = smoothstep(_CoreThickness * pulse * rippleModifier, 0.0, distFromCenter);
                
                // Tính toán hào quang bên ngoài (mờ và lan tỏa rộng hơn)
                float glow = smoothstep(_OuterThickness * pulse * rippleModifier, 0.0, distFromCenter);

                // Hòa trộn màu lõi và màu viền
                float4 col = _CoreColor * core + _OuterColor * glow;
                
                // Alpha phụ thuộc vào độ sáng của hào quang
                col.a = max(core, glow);
                
                // Làm mờ dần ở 2 đầu tia laser (trục X) để nhìn tự nhiên hơn
                float edgeFade = smoothstep(0.0, 0.1, i.uv.x) * smoothstep(1.0, 0.9, i.uv.x);
                col *= edgeFade;

                return col;
            }
            ENDCG
        }
    }
}
