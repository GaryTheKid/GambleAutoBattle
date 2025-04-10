using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PixelationRendererFeature : ScriptableRendererFeature
{
    [Serializable]
    public class PixelationSettings
    {
        [Range(1f, 64f)] public float pixelSize = 2.5f;  // Size of each pixel block in screen pixels
    }

    [SerializeField] private PixelationSettings settings = new PixelationSettings();
    [SerializeField] private Shader pixelationShader;

    private Material pixelationMaterial;
    private PixelationRenderPass pixelationPass;

    public override void Create()
    {
        if (pixelationShader == null)
        {
            Debug.LogError("PixelationRendererFeature: Shader not assigned.");
            return;
        }

        // Manually create the material and keep a reference to it
        if (pixelationMaterial == null)
            pixelationMaterial = new Material(pixelationShader);

        pixelationPass = new PixelationRenderPass(pixelationMaterial, settings)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
    }


    // This method is called every frame to allow the feature to enqueue the pass.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pixelationPass == null || pixelationMaterial == null)
            return;
        // Only apply to game cameras (avoid affecting Scene view or reflection cameras, etc.)
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(pixelationPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (pixelationMaterial != null)
        {
            Destroy(pixelationMaterial);
            pixelationMaterial = null;
        }
    }

}
