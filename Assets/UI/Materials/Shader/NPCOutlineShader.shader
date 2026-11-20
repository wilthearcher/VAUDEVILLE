
Shader "Custom/NPCOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0, 0.8, 1, 1)
        _OutlineThickness ("Outline Thickness", Range(0.001, 0.2)) = 0.05
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineThickness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 original = tex2D(_MainTex, i.uv);

                float2 offset = _MainTex_TexelSize.xy
              * (_OutlineThickness * 1000);

                float alpha = 0;

                alpha = max(alpha, tex2D(_MainTex, i.uv + float2(offset.x, 0)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv - float2(offset.x, 0)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv + float2(0, offset.y)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv - float2(0, offset.y)).a);

                alpha = max(alpha, tex2D(_MainTex, i.uv + float2(offset.x, offset.y)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv - float2(offset.x, offset.y)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv + float2(offset.x, -offset.y)).a);
                alpha = max(alpha, tex2D(_MainTex, i.uv + float2(-offset.x, offset.y)).a);

                fixed outlineAlpha = saturate(alpha - original.a);

                fixed4 result = original;

                result.rgb = lerp(
                    original.rgb,
                    _OutlineColor.rgb,
                    outlineAlpha
                );

                result.a = max(
                    original.a,
                    outlineAlpha * _OutlineColor.a
                );

                return result * i.color;
            }

            ENDCG
        }
    }
}
