using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SightRenderer : ScriptableRenderer
{
    DrawSightDistancePass m_SightDistancePass;
    RenderTargetHandle m_DepthTexture;
    Material m_SightDistanceMaterial;

    private static readonly string shaderName = "Hidden/SightDistance";
    
    public SightRenderer(SightRendererData data) : base(data)
    {
        var shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogWarning("dont find shader \"Hidden/SightDistance\"");
            return;
        }
        
        m_SightDistanceMaterial = new Material(shader);
        m_SightDistancePass = new DrawSightDistancePass(data.opaqueLayerMask, m_SightDistanceMaterial);
    }

    public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        Camera camera = renderingData.cameraData.camera;
        ref CameraData cameraData = ref renderingData.cameraData;
        RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

        EnqueuePass(m_SightDistancePass);

        for (int i = 0; i < rendererFeatures.Count; ++i)
        {
            if (rendererFeatures[i].isActive)
                rendererFeatures[i].AddRenderPasses(this, ref renderingData);
        }
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_SightDistanceMaterial);
    }
}

public class DrawSightDistancePass : ScriptableRenderPass
{
    private Material overrideMat;
    private FilteringSettings filtering;

    private static ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");
    
    public DrawSightDistancePass(LayerMask layerMask, Material mat)
    {
        overrideMat = mat;
        filtering = new FilteringSettings(RenderQueueRange.opaque, layerMask);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        DrawingSettings draw = CreateDrawingSettings(shaderTagId, ref renderingData, SortingCriteria.CommonOpaque);
        draw.overrideMaterial = overrideMat;
        draw.overrideMaterialPassIndex = 0;
        context.DrawRenderers(renderingData.cullResults, ref draw, ref filtering);
    }
}
