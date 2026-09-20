using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

public class LayerMaskRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public LayerMask layerMask;
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingOpaques;

        [Tooltip("Name of the global texture this pass exposes, e.g. _OutlineMaskTexture, _InteractableMaskTexture, etc.")]
        public string maskTextureName = "_MaskTexture";

        [Tooltip("Shader tag pass to draw (usually UniversalForward, sometimes SRPDefaultUnlit for unlit-only masks).")]
        public string shaderTagId = "UniversalForward";
    }

    public Settings settings = new Settings();
    public Shader maskShader;

    Material m_MaskMaterial;
    LayerMaskPass m_Pass;

    class LayerMaskPass : ScriptableRenderPass
    {
        Settings settings;
        Material maskMaterial;
        int maskTextureId;

        class PassData { public RendererListHandle rendererList; }

        public void Setup(Settings s, Material mat)
        {
            settings = s;
            maskMaterial = mat;
            renderPassEvent = s.passEvent;
            maskTextureId = Shader.PropertyToID(s.maskTextureName);
            profilingSampler = new ProfilingSampler(s.maskTextureName);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var renderingData = frameData.Get<UniversalRenderingData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            var desc = cameraData.cameraTargetDescriptor;

            var maskTex = renderGraph.CreateTexture(new TextureDesc(desc.width, desc.height)
            {
                colorFormat = GraphicsFormat.R8_UNorm,
                name = settings.maskTextureName,
                clearBuffer = true,
                clearColor = Color.clear
            });

            var sorting = new SortingSettings(cameraData.camera) { criteria = SortingCriteria.CommonOpaque };
            var drawing = new DrawingSettings(new ShaderTagId(settings.shaderTagId), sorting)
            {
                overrideMaterial = maskMaterial
            };
            var filtering = new FilteringSettings(RenderQueueRange.opaque, settings.layerMask);
            var listParams = new RendererListParams(renderingData.cullResults, drawing, filtering);
            var rendererList = renderGraph.CreateRendererList(listParams);

            using var builder = renderGraph.AddRasterRenderPass<PassData>($"{settings.maskTextureName} Pass", out var passData);
            passData.rendererList = rendererList;
            builder.UseRendererList(rendererList);
            builder.SetRenderAttachment(maskTex, 0, AccessFlags.Write);
            builder.SetGlobalTextureAfterPass(maskTex, maskTextureId);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                ctx.cmd.DrawRendererList(data.rendererList));
        }
    }

    public override void Create()
    {
        if (maskShader != null)
            m_MaskMaterial = CoreUtils.CreateEngineMaterial(maskShader);
        m_Pass = new LayerMaskPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (m_MaskMaterial == null) return;
        m_Pass.Setup(settings, m_MaskMaterial);
        renderer.EnqueuePass(m_Pass);
    }

    protected override void Dispose(bool disposing) => CoreUtils.Destroy(m_MaskMaterial);
}