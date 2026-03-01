Shader "Custom/EllipsePortalRing"
{
    Properties
    {
        _ColorInner ("Inner Glow Color", Color) = (1, 0.9, 0.5, 1)
        _ColorOuter ("Outer Glow Color", Color) = (1, 0.3, 0, 1)
        _Radius ("Portal Radius", Range(0.1, 0.5)) = 0.35
        _Thickness ("Ring Thickness", Range(0.01, 0.2)) = 0.05
        _GlowRange ("Glow Spread", Range(0.1, 0.5)) = 0.2
        _Intensity ("Intensity", Range(1, 10)) = 2.5
        _Speed ("Pulse Speed", Range(0, 5)) = 2.0
        
        // Thêm thông số độ dẹt
        _Squish ("Squish (Y-Axis)", Range(1.0, 5.0)) = 2.5 
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
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

            float4 _ColorInner;
            float4 _ColorOuter;
            float _Radius;
            float _Thickness;
            float _GlowRange;
            float _Intensity;
            float _Speed;
            float _Squish; // Biến mới để bóp dẹt hình

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Đưa UV về tâm (0,0)
                float2 centeredUV = i.uv - 0.5;
                
                // Bóp dẹt trục Y để tạo hình Elip
                centeredUV.y *= _Squish; 
                
                // Tính khoảng cách từ tâm đã bị bóp dẹt
                float dist = length(centeredUV);

                // Tạo hiệu ứng nhấp nháy
                float pulse = sin(_Time.y * _Speed) * 0.02;
                float currentRadius = _Radius + pulse;

                // Tính khoảng cách tới viền (tạo hình nhẫn)
                float ringDist = abs(dist - currentRadius);

                // Lõi sáng và viền Glow
                float coreMask = smoothstep(_Thickness, 0.0, ringDist);
                float glowMask = smoothstep(_GlowRange, 0.0, ringDist);

                // Pha màu
                float3 finalColor = lerp(_ColorOuter.rgb, _ColorInner.rgb, coreMask);
                
                // Cường độ sáng
                float totalMask = coreMask + (glowMask * 0.5);
                finalColor *= totalMask * _Intensity;

                return float4(finalColor, totalMask);
            }
            ENDCG
        }
    }
}