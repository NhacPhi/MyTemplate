Shader "Custom/VerticalEllipseWave"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("Color (Màu sắc)", Color) = (0.0, 1.0, 1.0, 1.0)
        
        [Header(Wave Settings)]
        _Speed ("Speed (Tốc độ)", Float) = 0.5
        _Count ("Wave Count (Số lượng sóng)", Int) = 5 // Tăng lên 5 vòng như ảnh
        
        [Header(Movement)]
        _StartX ("Start X (Vị trí bắt đầu)", Range(0, 1)) = 0.15
        _EndX ("End X (Vị trí kết thúc)", Range(0, 1)) = 0.85
        
        [Header(Shape and Scale)]
        // Tỉ lệ < 1 sẽ tạo hình elip đứng. 0.2 là rất dẹt như ảnh.
        _Aspect ("Aspect Ratio (Tỉ lệ dẹt X/Y)", Float) = 0.2 
        _StartScale ("Start Scale (Kích thước đầu)", Float) = 0.1
        _EndScale ("End Scale (Kích thước cuối)", Float) = 0.8
        _Thickness ("Line Thickness (Độ dày nét)", Range(0.001, 0.05)) = 0.02
    }

    SubShader 
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }
        
        Blend One One // Chế độ cộng màu (Additive) cho hiệu ứng phát sáng
        Cull Off 
        ZWrite Off

        Pass 
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { 
                float4 vertex : POSITION; 
                float2 uv : TEXCOORD0; 
                float4 color : COLOR; 
            };
            
            struct v2f { 
                float4 vertex : SV_POSITION; 
                float2 uv : TEXCOORD0; 
                float4 color : COLOR; 
            };

            float4 _Color;
            float _Speed, _StartX, _EndX, _Aspect, _StartScale, _EndScale, _Thickness;
            int _Count;

            v2f vert(appdata_t v) {
                v2f o; 
                o.vertex = UnityObjectToClipPos(v.vertex); 
                o.uv = v.uv; 
                o.color = v.color; 
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float finalRing = 0;
                
                // Vòng lặp tạo ra nhiều đợt sóng liên tục
                for(int j = 0; j < _Count; j++) 
                {
                    // 1. Tính vòng đời (t) của đợt sóng này (từ 0 đến 1)
                    // Offset thời gian để các sóng xuất hiện nối tiếp nhau
                    float timeOffset = (float)j / (float)_Count;
                    float t = frac(_Time.y * _Speed + timeOffset);
                    
                    // 2. Tính vị trí và kích thước hiện tại dựa trên vòng đời (t)
                    float currentX = lerp(_StartX, _EndX, t);
                    float currentScale = lerp(_StartScale, _EndScale, t);
                    
                    // 3. Đưa gốc tọa độ về tâm của đợt sóng
                    float2 center = float2(currentX, 0.5);
                    float2 p = i.uv - center;
                    
                    // 4. Tạo hình Elip đứng
                    // Chia p.x cho một số nhỏ (_Aspect < 1) sẽ làm trục X bị nén lại -> Elip đứng.
                    p.x /= max(_Aspect, 0.001);
                    
                    // Tính khoảng cách từ pixel đến tâm elip
                    float dist = length(p);
                    
                    // 5. Vẽ đường viền (Ring)
                    float halfThick = _Thickness * 0.5;
                    // Sử dụng smoothstep để tạo đường nét sắc sảo
                    float ring = smoothstep(currentScale + halfThick + 0.01, currentScale + halfThick, dist) 
                               * smoothstep(currentScale - halfThick - 0.01, currentScale - halfThick, dist);
                               
                    // 6. Hiệu ứng Fade (Mờ dần)
                    // Sóng sẽ hiện ra, rõ nhất ở giữa, rồi mờ dần khi kết thúc
                    float alphaFade = sin(t * 3.14159);
                    
                    // Cộng dồn vào kết quả cuối cùng
                    finalRing += ring * alphaFade;
                }
                
                // Áp dụng màu sắc và alpha
                return fixed4(_Color.rgb * finalRing, 1.0) * i.color.a;
            }
            ENDCG
        }
    }
}