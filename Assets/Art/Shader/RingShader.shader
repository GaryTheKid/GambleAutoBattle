Shader "Custom/URP/RingShaderTransparent"
{
    Properties
    {
        _RingColor("Ring Color", Color) = (1,1,1,1)
        _InnerRadius("Inner Radius", Float) = 0.2
        _OuterRadius("Outer Radius", Float) = 0.4
        _Smoothness("Edge Smoothness", Float) = 0.01
    }

        SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _RingColor;
            float _InnerRadius;
            float _OuterRadius;
            float _Smoothness;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Center of the ring = object's world position (center of plane)
                float3 planeCenterWS = mul(GetObjectToWorldMatrix(), float4(0.0, 0.0, 0.0, 1.0)).xyz;
                float2 pos = IN.worldPos.xz;
                float2 center = planeCenterWS.xz;

                float dist = distance(pos, center);

                float inner = smoothstep(_InnerRadius - _Smoothness, _InnerRadius + _Smoothness, dist);
                float outer = smoothstep(_OuterRadius - _Smoothness, _OuterRadius + _Smoothness, dist);
                float ringMask = outer - inner;

                // Apply alpha blending for transparency
                float4 ringColor = _RingColor;
                ringColor.a *= ringMask;

                return ringColor;
            }
            ENDHLSL
        }
    }
}
