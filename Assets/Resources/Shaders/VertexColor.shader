Shader "Shaders/VertexColor" {
    SubShader {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };
            
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                // 伽马校正（如果使用线性空间）
                #ifndef UNITY_COLORSPACE_GAMMA
                i.color.rgb = pow(i.color.rgb, 2.2);
                #endif
                return i.color;
            }
            ENDCG
        }
    }
}