using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class PixelationRenderPass : ScriptableRenderPass
{
    private static readonly int _PixelSizeID = Shader.PropertyToID("_PixelSize");
    private const string k_TempTargetName = "_PixelationTempTexture";

    private Material pixelationMaterial;
    private PixelationRendererFeature.PixelationSettings settings;
    private RenderTextureDescriptor tempDescriptor;

    public PixelationRenderPass(Material material, PixelationRendererFeature.PixelationSettings settings)
    {
        this.pixelationMaterial = material;
        this.settings = settings;
        // Initialize a descriptor for the temporary texture (we'll update resolution each frame)
        tempDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.Default, 0);
    }

    // This method is called by URP when using the RenderGraph system (URP 17+).
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // If the active render target is the backbuffer (screen), skip effect to avoid invalid blit&#8203;:contentReference[oaicite:3]{index=3}.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        // Update the temporary RT descriptor to current camera dimensions (handle dynamic resolution/XR)
        tempDescriptor.width = cameraData.cameraTargetDescriptor.width;
        tempDescriptor.height = cameraData.cameraTargetDescriptor.height;
        tempDescriptor.depthBufferBits = 0;

        // Get a handle to the current camera color buffer
        TextureHandle source = resourceData.activeColorTexture;
        // Create a temporary render graph texture for our effect¡¯s intermediate result
        TextureHandle tempTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, tempDescriptor,
                                                                               k_TempTargetName, false);

        // Set the pixel size parameter on the material each frame (using either volume override or default)
        pixelationMaterial.SetFloat(_PixelSizeID, Mathf.Max(0.1f, settings.pixelSize));

        // Ensure source and destination handles are valid before adding passes
        if (!source.IsValid() || !tempTexture.IsValid())
            return;

        // Add a blit pass to apply the pixelation shader from source to the temp texture&#8203;:contentReference[oaicite:4]{index=4}
        var pixelatePass = new RenderGraphUtils.BlitMaterialParameters(source, tempTexture, pixelationMaterial, 0);
        renderGraph.AddBlitPass(pixelatePass, "PixelatePass");

        // Add a second blit pass to copy the pixelated result back to the camera's color buffer&#8203;:contentReference[oaicite:5]{index=5}.
        var copyBackPass = new RenderGraphUtils.BlitMaterialParameters(tempTexture, source, pixelationMaterial, 0);
        renderGraph.AddBlitPass(copyBackPass, "PixelateCopyBack");
    }
}
