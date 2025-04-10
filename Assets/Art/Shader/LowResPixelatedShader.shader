Shader "Custom/PostProcessing/PixelationEffect"
{
    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // Include Blit utilities: defines the full-screen triangle vertex shader (Vert) 
        // and Varyings structure with UV coordinates.
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    // Pixelation parameter (block size in pixels)
    // (Will be set from C# script via material property)
    float _PixelSize;

    // Fragment shader: Pixelate the image by sampling at a reduced resolution
    float4 FragPixelate(Varyings input) : SV_Target
    {
        // Get screen resolution (width, height) from Unity built-in _ScreenParams
        float2 screenSize = float2(_ScreenParams.x, _ScreenParams.y);

        // Compute the UV coordinates corresponding to the top-left pixel of the current block
        float2 pixelCoord = input.texcoord * screenSize;
        float2 blockCoord = floor(pixelCoord / _PixelSize) * _PixelSize;
        // Offset by 0.5 to sample at pixel center (ensures we pick one pixel's color)
        float2 sampleCoord = blockCoord + 0.5;
        float2 sampleUV = sampleCoord / screenSize;

        // Sample the source texture at the computed UV (using linear sampling, but since UV is at the pixel center of a block, it yields a single pixel¡¯s color)
        float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, sampleUV);
        return color;
    }

        ENDHLSL

        SubShader
    {
        Tags{ "RenderPipeline" = "UniversalPipeline" }
            Pass
        {
            Name "PixelatePass"
            ZWrite Off Cull Off ZTest Always  // Full-screen, no depth needed
            HLSLPROGRAM

                #pragma vertex Vert          // Use provided full-screen blit vertex shader
                #pragma fragment FragPixelate

            ENDHLSL
        }
    }
}