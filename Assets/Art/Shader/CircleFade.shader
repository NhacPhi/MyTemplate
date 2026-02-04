Shader "Custom/CircleFade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Alpha ("Fade Amount", Range(0.0, 1.0)) = 0.0 // Thanh trượt từ 0 đến 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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

            fixed4 _Color;
            float _Alpha; 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float circle(float2 position, float radius, float feather)
            {
                return smoothstep(radius, radius + feather, length(position - float2(0.5, 0.5)));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float outer = circle(i.uv, 0.35, 0.01);
                float fade_effect = sin(_Time.y) * 0.01;
                float inner = 1.0 - circle(i.uv, 0.275, 0.1 - fade_effect);
                
                fixed4 col = _Color;
                
                float circle_alpha = 1.0 - (outer + inner);

                col.a = circle_alpha * _Alpha;
                
                return col;
            }
            ENDCG
        }
    }
}