Shader "UI/CircleSelectionGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        
        _Radius ("Radius", Range(0.0, 0.5)) = 0.4
        _Thickness ("Thickness", Range(0.01, 0.2)) = 0.03
        
        [HDR] _GlowColor ("Glow Color (HDR)", Color) = (0, 2, 2, 1)
        _GlowRange ("Glow Range", Range(0.01, 0.5)) = 0.1
        _PulseSpeed ("Pulse Speed", Float) = 3.0
        
        // Cấu hình bắt buộc cho UI Mask
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha One
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _GlowColor;
            float _Radius;
            float _Thickness;
            float _GlowRange;
            float _PulseSpeed;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord - 0.5;
                float dist = length(uv);

                // Tạo hiệu ứng Pulse (Nhịp điệu) dựa trên thời gian thực
                float pulse = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5);
                float animRadius = _Radius + (pulse * 0.02);

                // Tính khoảng cách tới vòng tròn
                float circleDist = abs(dist - animRadius);

                // Phần lõi (Core) của vòng tròn
                float core = smoothstep(_Thickness, _Thickness * 0.5, circleDist);
                
                // Phần tỏa sáng (Glow) - Sử dụng hàm mũ để tạo độ suy giảm mượt
                float glow = exp(-circleDist / _GlowRange);
                glow *= (0.5 + pulse * 0.5); // Glow cũng nhấp nháy theo nhịp

                // Kết hợp màu sắc
                fixed4 finalCol = (i.color * core) + (_GlowColor * glow);
                
                // Alpha xử lý theo độ sáng của Glow
                finalCol.a = saturate(core + glow) * i.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                finalCol.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                return finalCol;
            }
            ENDCG
        }
    }
}