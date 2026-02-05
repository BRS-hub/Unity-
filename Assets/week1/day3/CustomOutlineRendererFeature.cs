using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// 示例：自定义Renderer Feature，用于渲染特定图层的物体轮廓
// 将此脚本添加到URP Renderer的Renderer Features列表
public class CustomOutlineRendererFeature : ScriptableRendererFeature
{
    // 可配置参数
    [System.Serializable]
    public class Settings
    {
        public LayerMask outlineLayer = 1; // 默认图层
        public Material overrideMaterial;  // 轮廓材质
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public Settings settings = new Settings();
    private CustomOutlineRenderPass outlinePass;

    public override void Create()
    {
        outlinePass = new CustomOutlineRenderPass(settings);
    }

    // 每帧调用，将Render Pass添加到渲染队列
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.overrideMaterial == null)
        {
            Debug.LogWarning("Outline material is not assigned.");
            return;
        }
        renderer.EnqueuePass(outlinePass);
    }

    // 自定义Render Pass类
    private class CustomOutlineRenderPass : ScriptableRenderPass
    {
        private Settings settings;
        private FilteringSettings filteringSettings;
        private RenderStateBlock renderStateBlock;

        public CustomOutlineRenderPass(Settings settings)
        {
            this.settings = settings;
            this.filteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.outlineLayer);
            this.renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            this.renderPassEvent = settings.renderEvent;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 绘制指定图层的物体，使用覆盖材质
            var drawSettings = new DrawingSettings(new ShaderTagId("UniversalForward"),
                new SortingSettings(renderingData.cameraData.camera))
            {
                overrideMaterial = settings.overrideMaterial,
                overrideMaterialPassIndex = 0
            };

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings, ref renderStateBlock);
        }
    }
}

// 使用说明：
// 1. 在Unity中，打开Universal Render Pipeline Asset → Renderer List → Forward Renderer。
// 2. 在Renderer Features中添加此CustomOutlineRendererFeature。
// 3. 创建一个轮廓材质（如使用Shader Graph制作边缘发光效果）。
// 4. 将轮廓材质拖入Feature的Override Material槽。
// 5. 设置Outline Layer为需要显示轮廓的物体所在图层。
// 6. 运行场景，观察轮廓效果。