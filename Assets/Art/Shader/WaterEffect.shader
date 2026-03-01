Shader "Custom/WaterSphere2D"
{
    Properties
    {
        [Header(Water Colors)]
        _WaterColor ("Water Base Color", Color) = (0.2, 0.7, 1.0, 0.9)
        _DeepColor ("Deep Water Color", Color) = (0.0, 0.4, 0.8, 0.9)
        _HighlightColor ("Highlight Color", Color) = (1.0, 1.0, 1.0, 0.8)

        [Header(Freeze Effect)]
        _FreezeAmount ("Freeze Amount", Range(0.0, 1.0)) = 0.0 
        [HDR] _IceColor ("Ice Base Color", Color) = (0.5, 0.85, 1.0, 0.95) 
        [HDR] _FrostColor ("Frost Pattern Color", Color) = (0.9, 0.95, 1.0, 1.0) 

        [Header(Shatter Effect (Ice Break))]
        _ShatterAmount ("Shatter Amount", Range(0.0, 1.0)) = 0.0 // Kéo để đập vỡ
        _ShatterScale ("Shatter Shards Count", Float) = 8.0 // Số lượng mảnh vỡ

        [Header(Dissolve Effect (Evaporate))]
        _DissolveAmount ("Dissolve Amount", Range(0.0, 1.0)) = 0.0 
        [HDR] _DissolveEdgeColor ("Dissolve Edge Color", Color) = (0.5, 1.0, 1.0, 1.0) 
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0.0, 0.1)) = 0.03 
        _DissolveScale ("Dissolve Noise Scale", Float) = 5.0 

        [Header(Motion and Shape)]
        _GlobalAlpha ("Global Alpha (Opacity)", Range(0.0, 1.0)) = 1.0 
        _WobbleSpeed ("Wobble Speed", Float) = 3.0
        _WobbleStrength ("Wobble Strength", Range(0, 0.2)) = 0.05
        _WaveSpeed ("Inner Wave Speed", Float) = 1.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Khai báo biến
            float4 _WaterColor, _DeepColor, _HighlightColor;
            float _WobbleSpeed, _WobbleStrength, _WaveSpeed, _GlobalAlpha; 
            float _FreezeAmount;
            float4 _IceColor, _FrostColor;
            float _DissolveAmount, _DissolveEdgeWidth, _DissolveScale;
            float4 _DissolveEdgeColor;
            float _ShatterAmount, _ShatterScale;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Hàm nhiễu ngẫu nhiên 1D
            float hash(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
            
            // Hàm nhiễu Vector 2D (Dùng cho cắt mảnh Voronoi)
            float2 hash22(float2 p) {
                float3 p3 = frac(float3(p.xyx) * float3(.1031, .1030, .0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx+p3.yz)*p3.zy);
            }

            // Hàm Value Noise (Dùng cho nước và tan biến)
            float noise(float2 p) {
                float2 i = floor(p); float2 f = frac(p);
                f = f*f*(3.0-2.0*f);
                return lerp(lerp(hash(i), hash(i+float2(1,0)), f.x),
                            lerp(hash(i+float2(0,1)), hash(i+float2(1,1)), f.x), f.y);
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 centeredUV = i.uv * 2.0 - 1.0;

                // ==========================================
                // 1. TẠO MẢNH VỠ (SHATTER VORONOI)
                // ==========================================
                float2 voronoiUV = centeredUV * _ShatterScale;
                float2 g = floor(voronoiUV);
                float2 f = frac(voronoiUV);
                float minDist = 100.0;
                float2 bestCell = float2(0,0);

                // Quét 9 ô xung quanh để tìm mảnh vỡ gần nhất (Voronoi)
                for(int y=-1; y<=1; y++) {
                    for(int x=-1; x<=1; x++) {
                        float2 lattice = float2(x, y);
                        float2 offset = hash22(g + lattice);
                        float d = length(lattice + offset - f);
                        if(d < minDist) {
                            minDist = d;
                            bestCell = g + lattice;
                        }
                    }
                }
                
                // Mã ID ngẫu nhiên cho từng mảnh vỡ
                float shardHash = hash(bestCell); 

                // Mảnh vỡ sẽ từ từ biến mất ngẫu nhiên khi nổ
                if (_ShatterAmount > 0.0 && shardHash < _ShatterAmount - 0.2) discard;

                float2 sphereUV = centeredUV;
                
                if (_ShatterAmount > 0.0) {
                    // Tính hướng văng ra từ tâm cho từng mảnh
                    float2 explodeDir = normalize((bestCell / _ShatterScale) + 0.001); 
                    
                    // Khoảng cách văng mạnh/yếu tùy vào shardHash
                    float2 shatterOffset = explodeDir * _ShatterAmount * (shardHash + 0.3) * 1.5;
                    
                    // Lực hút Trái Đất kéo mảnh vỡ rơi xuống
                    float2 gravity = float2(0.0, -1.0) * pow(_ShatterAmount, 2.0) * 1.5;

                    // Di chuyển tọa độ UV để tách các mảnh ra
                    sphereUV -= (shatterOffset + gravity);
                    
                    // Tạo các khe nứt (Cracks) mỏng giữa các mảnh vỡ
                    if (minDist > 0.9 - _ShatterAmount * 0.1) discard; 
                }

                // ==========================================
                // 2. CHUYỂN ĐỘNG & ĐÓNG BĂNG (Sử dụng sphereUV bị vỡ)
                // ==========================================
                float dist = length(sphereUV);
                float angle = atan2(sphereUV.y, sphereUV.x);

                float wobble = sin(angle * 4.0 + _Time.y * _WobbleSpeed) * 0.5 + 
                               cos(angle * 7.0 - _Time.y * _WobbleSpeed * 0.7) * 0.5;
                wobble *= (1.0 - _FreezeAmount); 
                
                float currentRadius = 0.85 + wobble * _WobbleStrength;

                float circleMask = smoothstep(currentRadius + 0.02, currentRadius - 0.02, dist);
                if (circleMask <= 0.0) discard;

                float2 waveUV = sphereUV * 1.5;
                waveUV.y += sin(waveUV.x * 2.0 + _Time.y * _WaveSpeed) * 0.2 * (1.0 - _FreezeAmount);
                float movingTime = _Time.y * (1.0 - _FreezeAmount); 
                
                float n = noise(waveUV + float2(movingTime * 0.5, movingTime * 0.2));
                float4 liquidColor = lerp(_DeepColor, _WaterColor, n);

                // Vân tinh thể băng
                float frostNoise = noise(sphereUV * 12.0); 
                float frostPattern = smoothstep(0.4, 0.6, frostNoise); 
                float4 solidIceColor = lerp(_IceColor, _FrostColor, frostPattern);

                float4 baseColor = lerp(liquidColor, solidIceColor, _FreezeAmount);

                // ==========================================
                // 3. HIỆU ỨNG TAN BIẾN BỐC HƠI (DISSOLVE)
                // ==========================================
                float dissolveNoise = noise(sphereUV * _DissolveScale);
                if (dissolveNoise < _DissolveAmount) discard;
                float isDissolveEdge = step(dissolveNoise - _DissolveAmount, _DissolveEdgeWidth);

                // ==========================================
                // 4. KHỐI 3D VÀ ÁNH SÁNG
                // ==========================================
                float edgeGlow = smoothstep(currentRadius - 0.2, currentRadius, dist);
                float3 currentGlowColor = lerp(_WaterColor.rgb, _FrostColor.rgb, _FreezeAmount);
                baseColor.rgb += currentGlowColor * edgeGlow * 0.6;

                float2 highlightCenter = float2(-0.25, 0.35); 
                float2 highlightUV = sphereUV - highlightCenter;
                float highDist1 = length(highlightUV);
                float highDist2 = length(sphereUV - float2(-0.15, 0.25)); 
                float highlightMask = smoothstep(0.4, 0.25, highDist1) * smoothstep(0.2, 0.4, highDist2);

                float4 finalColor = baseColor;
                finalColor.rgb = lerp(finalColor.rgb, _HighlightColor.rgb, highlightMask * _HighlightColor.a);
                finalColor.rgb = lerp(finalColor.rgb, _DissolveEdgeColor.rgb, isDissolveEdge * step(0.01, _DissolveAmount));
                
                finalColor.a = circleMask * baseColor.a * _GlobalAlpha; 

                return finalColor;
            }
            ENDCG
        }
    }
}