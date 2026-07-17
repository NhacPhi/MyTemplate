Shader "Custom/IceSnowBreath"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [Header(Color Settings)]
        _CoreColor ("Core Color (Màu lõi)", Color) = (0.85, 0.95, 1.0, 1.0)
        _OuterColor ("Outer Color (Màu rìa)", Color) = (0.2, 0.6, 1.0, 0.7)
        _GlowIntensity ("Glow Intensity (Cường độ phát sáng)", Range(0.5, 3.0)) = 2.5

        [Header(Wind and Movement)]
        _WindSpeed ("Wind Speed & Direction (X, Y)", Vector) = (2.5, 0.4, 0, 0)
        _NoiseScale ("Wind Noise Scale", Float) = 2.2
        _Distortion ("Wind Distortion Strength", Range(0.0, 2.0)) = 0.6

        [Header(Ice Crystal and Snowflake Particles)]
        _CrystalDensity ("Crystal Density (0-1)", Range(0.0, 1.0)) = 0.7
        _CrystalScale ("Crystal Grid Scale", Float) = 7.0
        _CrystalSpeed ("Crystal Sparkle Speed", Float) = 10.0
        _CrystalColor ("Crystal Color", Color) = (0.9, 0.98, 1.0, 1.0)

        [Header(Fading Settings)]
        _EdgeFadePower ("Edge Fade Power (Độ mềm biên)", Range(1.0, 6.0)) = 1.3
        _LengthFadeStart ("Length Fade Start (0-1)", Range(0.0, 0.5)) = 0.05
        _LengthFadeEnd ("Length Fade End (0-1)", Range(0.5, 1.0)) = 0.95

        [Header(Funnel Shape Settings)]
        _FunnelStartWidth ("Funnel Start Width (at X=0)", Range(0.05, 2.0)) = 0.2
        _FunnelEndWidth ("Funnel End Width (at X=1)", Range(0.05, 2.0)) = 1.0
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }
        
        Blend SrcAlpha One // Additive/Screen blend works best for blowing ice breath
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

            // Material Uniforms
            sampler2D _MainTex;
            float4 _CoreColor;
            float4 _OuterColor;
            float _GlowIntensity;
            float4 _WindSpeed;
            float _NoiseScale;
            float _Distortion;
            float _CrystalDensity;
            float _CrystalScale;
            float _CrystalSpeed;
            float4 _CrystalColor;
            float _EdgeFadePower;
            float _LengthFadeStart;
            float _LengthFadeEnd;
            float _FunnelStartWidth;
            float _FunnelEndWidth;

            // Pseudo-random noise functions
            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 fp = frac(p);
                fp = fp * fp * (3.0 - 2.0 * fp); // smoothstep interpolation
                
                float a = hash(i + float2(0.0, 0.0));
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, fp.x), lerp(c, d, fp.x), fp.y);
            }

            // Fractal Brownian Motion for wind gas clouds
            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                float2 shift = float2(100.0, 100.0);
                float2x2 rot = float2x2(0.8, 0.6, -0.6, 0.8);
                for (int i = 0; i < 4; ++i)
                {
                    v += a * noise(p);
                    p = mul(rot, p) * 2.0 + shift;
                    a *= 0.5;
                }
                return v;
            }

            // Twinkling Snowflake Crystals Generator
            float sparkles(float2 p, float speed, float density)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float sparkleSum = 0.0;
                
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 cell = ip + neighbor;
                        float h = hash(cell);
                        
                        // Check density threshold
                        if (h > density) continue;
                        
                        // Animate particle position slightly within its cell
                        float2 offset = float2(hash(cell + 13.57), hash(cell + 45.21));
                        float2 crystalPos = neighbor + offset - fp;
                        
                        // Sparkle flicker
                        float flicker = sin(_Time.y * speed + h * 62.83) * 0.5 + 0.5;
                        
                        // Shape: Star cross + sharp core dot
                        float dist = length(crystalPos);
                        float crossShape = (1.0 - smoothstep(0.005, 0.07, abs(crystalPos.x) * abs(crystalPos.y))) 
                                           * (1.0 - smoothstep(0.0, 0.2, dist));
                        float dotShape = smoothstep(0.12, 0.0, dist);
                        
                        sparkleSum += (dotShape + crossShape * 1.5) * flicker;
                    }
                }
                return saturate(sparkleSum);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; // Support for particle color/alpha fading
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate Object World Scale to prevent stretching/distortion
                float scaleX = length(mul(unity_ObjectToWorld, float4(1.0, 0.0, 0.0, 0.0)).xyz);
                float scaleY = length(mul(unity_ObjectToWorld, float4(0.0, 1.0, 0.0, 0.0)).xyz);
                scaleX = max(0.001, scaleX);
                scaleY = max(0.001, scaleY);

                // Calculate Funnel Width based on X coordinate
                float currentWidth = lerp(_FunnelStartWidth, _FunnelEndWidth, i.uv.x);
                currentWidth = max(0.001, currentWidth);

                // Remap local UV.y relative to the funnel width. Center remains at 0.5.
                float localY = (i.uv.y - 0.5) / currentWidth + 0.5;
                float2 funnelUV = float2(i.uv.x, localY);

                // Multiply UV by scale to keep noise coordinates in constant world space size
                float2 scaledUV = funnelUV * float2(scaleX, scaleY);
                
                // 1. Dynamic UV distortion to simulate swirling, gusty blizzard wind
                // Read noise to offset UV coordinates
                float distNoise = fbm(scaledUV * (_NoiseScale * 0.5) - _Time.y * _WindSpeed.xy * 0.3);
                
                float2 windUV = funnelUV + float2(distNoise, -distNoise) * _Distortion * 0.15;
                float2 windUV_scaled = windUV * float2(scaleX, scaleY);
                
                // 2. Wind blast/gas density calculation
                float2 scrollUV = windUV_scaled * _NoiseScale - _Time.y * _WindSpeed.xy;
                float windPattern = fbm(scrollUV);
                
                // 3. Fades and Masks
                // Edge fading (using the remapped localY clamped to 0-1)
                float edgeMask = saturate(sin(saturate(localY) * 3.14159));
                edgeMask = pow(edgeMask, _EdgeFadePower);
                
                // Length fading (fades out at the beginning and the end of the breath stream)
                float lengthMask = smoothstep(0.0, _LengthFadeStart, i.uv.x) * smoothstep(1.0, _LengthFadeEnd, i.uv.x);
                
                float finalMask = edgeMask * lengthMask;
                
                // 4. Color Interpolation (Outer icy blue to Core white-blue)
                float coreWeight = pow(windPattern, 1.8);
                float4 baseColor = lerp(_OuterColor, _CoreColor, coreWeight) * _GlowIntensity;
                
                // Apply the density mask to the wind blast base
                float4 windBlast = baseColor * windPattern * finalMask;
                
                // 5. Snowflake / Ice Crystals layer
                // Crystals scroll fast along the wind direction (using scaled UVs to maintain aspect ratio)
                float2 crystalUV = windUV_scaled * _CrystalScale - _Time.y * _WindSpeed.xy * 1.5;
                float crystalMask = sparkles(crystalUV, _CrystalSpeed, _CrystalDensity);
                
                // Boost crystals in the dense/main stream parts
                crystalMask *= (windPattern * 0.8 + 0.2) * finalMask;
                float4 crystalsColor = _CrystalColor * crystalMask * 2.0;
                
                // 6. Combine wind blast & crystals, multiply by vertex color for system compatibility
                float4 finalColor = (windBlast + crystalsColor) * i.color;
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Particles/Additive"
}
