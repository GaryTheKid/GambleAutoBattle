Shader "Custom/URP/RadialRingShader"
{
    Properties
    {
        _RingColor("Ring Color", Color) = (1,1,1,1)
        _InnerRadius("Inner Radius", Float) = 0.2
        _OuterRadius("Outer Radius", Float) = 0.4
        _Smoothness("Edge Smoothness", Float) = 0.01
        _FillAmount("Fill Amount [0-1]", Range(0,1)) = 1.0
        _Clockwise("Clockwise? (0=CCW, 1=CW)", Float) = 1.0
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
                float3 localPos : TEXCOORD1;
            };

            float4 _RingColor;
            float _InnerRadius;
            float _OuterRadius;
            float _Smoothness;
            float _FillAmount;
            float _Clockwise;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS);
                OUT.localPos = IN.positionOS.xyz;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 localDir = IN.localPos.xz;
                float dist = length(localDir);

                // Correct polar angle starting from 12 o'clock
                float angle = atan2(localDir.x, localDir.y);
                angle = angle < 0 ? angle + 6.2831853 : angle;
                float normAngle = angle / 6.2831853;

                float fillCheck = _Clockwise > 0.5 ? 1.0 - normAngle : normAngle;
                float angleMask = step(fillCheck, _FillAmount);

                // Fixed radius naming
                float innerEdge = smoothstep(_InnerRadius - _Smoothness, _InnerRadius + _Smoothness, dist);
                float outerEdge = smoothstep(_OuterRadius - _Smoothness, _OuterRadius + _Smoothness, dist);
                float ringMask = (innerEdge - outerEdge) * angleMask;

                float4 color = _RingColor;
                color.a *= ringMask;

                return color;
            }
            ENDHLSL
        }
    }
}
