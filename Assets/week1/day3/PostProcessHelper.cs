using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// URP后处理辅助脚本
// 提供常用后处理效果的快速实现示例
public class PostProcessHelper : MonoBehaviour
{
    [Header("灰度后处理")]
    public bool enableGrayscale = false;
    [Range(0f, 1f)]
    public float grayscaleIntensity = 1f;
    private Material grayscaleMaterial;

    [Header("边缘检测后处理")]
    public bool enableEdgeDetection = false;
    [Range(0f, 1f)]
    public float edgeThreshold = 0.2f;
    private Material edgeDetectionMaterial;

    private FullScreenPassRendererFeature grayscaleFeature;
    private FullScreenPassRendererFeature edgeDetectionFeature;

    void Start()
    {
        // 加载预制的后处理材质（需提前创建）
        grayscaleMaterial = LoadMaterial("GrayscalePostProcess");
        edgeDetectionMaterial = LoadMaterial("EdgeDetectionPostProcess");

        // 获取或创建Renderer Features
        SetupFeatures();
    }

    void Update()
    {
        UpdateGrayscale();
        UpdateEdgeDetection();
    }

    // 动态更新灰度效果参数
    void UpdateGrayscale()
    {
        if (grayscaleMaterial != null && enableGrayscale)
        {
            grayscaleMaterial.SetFloat("_Intensity", grayscaleIntensity);
        }
    }

    // 动态更新边缘检测参数
    void UpdateEdgeDetection()
    {
        if (edgeDetectionMaterial != null && enableEdgeDetection)
            edgeDetectionMaterial.SetFloat("_Threshold", edgeThreshold);
    }


// 加载后处理材质
    Material LoadMaterial(string materialName)
    {
        Material mat = Resources.Load<Material>("PostProcess/" + materialName);
        if (mat == null)
            Debug.LogWarning($"Material {materialName} not found in Resources/PostProcess/.");
        return mat;
    }

    // 配置Renderer Features（需在URP Renderer中预先添加FullScreenPassRendererFeature）
    void SetupFeatures()
    {
        // 获取URP Renderer
        var pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        if (pipeline == null) return;

        // 这里简化处理：实际项目中应通过编辑器配置或动态添加Feature
        Debug.Log("请在URP Renderer中手动添加FullScreenPassRendererFeature，并指定对应的材质。");
    }

    // 快速启用/禁用灰度效果
    public void ToggleGrayscale(bool enable)
    {
        enableGrayscale = enable;
        Debug.Log($"Grayscale {(enable ? "enabled" : "disabled")}");
    }

    // 快速启用/禁用边缘检测
    public void ToggleEdgeDetection(bool enable)
    {
        enableEdgeDetection = enable;
        Debug.Log($"Edge detection {(enable ? "enabled" : "disabled")}");
    }
}

// Shader Graph创建指南：
// 1. 灰度Shader Graph：
//    - 创建新Shader Graph，添加Sample Texture 2D节点，输入为_MainTex。
//    - 添加Dot Product节点，计算RGB与固定向量(0.299, 0.587, 0.114)的点积，得到灰度值。
//    - 使用Lerp节点混合原色与灰度值，混合系数由_Intensity控制。
//    - 输出到Base Color。
//
// 2. 边缘检测Shader Graph（Sobel算子）：
//    - 使用9个Sample Texture 2D节点，采样_MainTex及其周围8个偏移像素。
//    - 应用Sobel卷积核计算横向梯度Gx和纵向梯度Gy。
//    - 边缘强度 = sqrt(Gx^2 + Gy^2)，与_Threshold比较决定输出。
//    - 输出边缘为白色，其余为黑色。
//
// 3. URP Renderer配置：
//    - 将Shader Graph保存后，创建对应材质。
//    - 在URP Renderer的Renderer Features中添加FullScreenPassRendererFeature。
//    - 将材质拖入Pass Material槽，选择适当的Render Event（如AfterRenderingPostProcessing）。