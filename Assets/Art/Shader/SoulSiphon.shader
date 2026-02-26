Shader "Custom/SpriteSoulSiphon_SuctionFixed"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Soul Settings)]
        [HDR] _SoulColor ("Soul Color (HDR)", Color) = (0.0, 1.0, 0.8, 1.0)
        
        [Header(Vortex Settings)]
        _Center ("Center Point", Vector) = (0.5, 0.5, 0, 0)
        _Suction ("Suction Strength", Range(0, 5)) = 1.5    
        _Twist ("Twist Strength", Float) = 5.0              
        _Distortion ("Physical Distortion", Range(0, 1)) = 0.2 
        
        [Header(Animation)]
        _Speed ("Flow Speed", Float) = 2.0                  
        _Turbulence ("Turbulence", Range(0, 1)) = 0.5       
        _NoiseScale ("Noise Scale", Float) = 3.0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha One

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
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            
            float4 _SoulColor;
            float2 _Center;
            float _Suction;
            float _Twist;
            float _Distortion;
            float _Speed;
            float _Turbulence;
            float _NoiseScale;

            // --- HÀM NOISE ---
            float hash(float2 p) { return frac(1e4 * sin(17.0 * p.x + p.y * 0.1) * (0.1 + abs(sin(p.y * 13.0 + p.x)))); }
            
            float noise(float2 x) {
                float2 i = floor(x);
                float2 f = frac(x);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i), hash(i + float2(1.0, 0.0)), u.x),
                            lerp(hash(i + float2(0.0, 1.0)), hash(i + float2(1.0, 1.0)), u.x), u.y);
            }

            float fbm(float2 x) {
                float v = 0.0; float a = 0.5; float2 shift = float2(100, 100);
                float2x2 rot = float2x2(cos(0.5), sin(0.5), -sin(0.5), cos(0.5));
                for (int i = 0; i < 3; ++i) {
                    v += a * noise(x); x = mul(x, rot) * 2.0 + shift; a *= 0.5;
                }
                return v;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. TÍNH TOÁN TỌA ĐỘ CỰC
                float2 centeredUV = IN.texcoord - _Center; 
                float r = length(centeredUV);              
                float a = atan2(centeredUV.y, centeredUV.x);       

                // 2. TẠO DÒNG CHẢY NOISE (ĐÃ SỬA LOGIC)
                float suctionEffect = pow(r, 1.0 / max(_Suction, 0.01));
                
                // THAY ĐỔI 1: Đảo chiều xoắn để tạo cảm giác cuốn vào trong
                float twistEffect = a - (1.0 - r) * _Twist; // Dùng dấu trừ (-) thay vì cộng
                
                // THAY ĐỔI 2: Đổi dấu thời gian (+ _Time.y) để dòng chảy đi ngược lại (vào tâm)
                float2 noiseUV = float2(twistEffect * 2.0, suctionEffect * 5.0 + _Time.y * _Speed);
                
                noiseUV += fbm(centeredUV * _NoiseScale + _Time.y) * _Turbulence;

                float soulNoise = fbm(noiseUV);
                
                // 3. BIẾN DẠNG VẬT THỂ
                // THAY ĐỔI 3: Đảo dấu méo hình (0.5 - soulNoise) để hướng méo đồng bộ với hướng hút
                float2 distortOffset = (centeredUV / (r + 0.01)) * (0.5 - soulNoise) * _Distortion;
                
                float textureTwistAngle = a + (1.0 - smoothstep(0.0, 1.0, r)) * _Twist * _Distortion * 0.5;
                float2 twistedUV = _Center + float2(cos(textureTwistAngle), sin(textureTwistAngle)) * r;

                float2 finalUV = twistedUV + distortOffset;

                // 4. LẤY MẪU ẢNH
                fixed4 spriteCol = tex2D(_MainTex, finalUV);

                // --- MÀU SẮC ---
                soulNoise = smoothstep(0.2, 0.8, soulNoise);
                float coreGlow = 1.0 / (r * 10.0 + 0.1); 
                float3 finalRGB = _SoulColor.rgb * soulNoise;
                finalRGB += _SoulColor.rgb * coreGlow * 0.5;

                float finalAlpha = spriteCol.a * IN.color.a * soulNoise;
                finalAlpha *= smoothstep(0.5, 0.2, r);

                finalRGB += spriteCol.rgb * spriteCol.a * 0.5;

                return fixed4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
}