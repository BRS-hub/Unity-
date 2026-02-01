using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTransformController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 90.0f; // 度/秒
    public float scaleSpeed = 0.1f;

    [Header("缩放限制")]
    public float minScale = 0.5f;
    public float maxScale = 3.0f;

    private Vector3 currentScale;

    void Start()
    {
        // 记录初始缩放，避免无限缩放
        currentScale = transform.localScale;
        Debug.Log("基础变换控制器已启动");
    }

    void Update()
    {
        // 任务1：处理平移输入
        HandleTranslation();

        // 任务2：处理旋转输入
        HandleRotation();

        // 任务3：处理缩放输入
        HandleScaling();
    }

    // 待实现的方法...
    void HandleTranslation()
    {
        // 获取输入（WASD标准移动控制）
        float horizontal = Input.GetAxis("Horizontal"); // A/D 对应 -1/1
        float vertical = Input.GetAxis("Vertical");     // S/W 对应 -1/1

        // 创建移动向量（XZ平面移动，Y轴不变）
        Vector3 movement = new Vector3(horizontal, 0, vertical);

        // 归一化防止对角线移动过快
        if (movement.magnitude > 1)
            movement = movement.normalized;

        // 应用移动（与帧率无关）
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        // 可选：输出当前位置
        if (movement.magnitude > 0.01f)
            Debug.Log($"当前位置: {transform.position}");
    }

    void HandleRotation()
    {
        // Q/E键旋转控制
        float rotationInput = 0f;

        if (Input.GetKey(KeyCode.Q))
            rotationInput = -1f; // 逆时针
        else if (Input.GetKey(KeyCode.E))
            rotationInput = 1f;  // 顺时针

        // 应用旋转（绕Y轴）
        float rotationAmount = rotationInput * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotationAmount, 0, Space.Self);
    }

    void HandleScaling()
    {
        // R/F键缩放控制
        float scaleInput = 0f;

        if (Input.GetKey(KeyCode.R))
            scaleInput = 1f;  // 放大
        else if (Input.GetKey(KeyCode.F))
            scaleInput = -1f; // 缩小

        if (scaleInput != 0)
        {
            // 计算新缩放值
            currentScale += Vector3.one * scaleInput * scaleSpeed;

            // 钳制缩放范围
            currentScale.x = Mathf.Clamp(currentScale.x, minScale, maxScale);
            currentScale.y = Mathf.Clamp(currentScale.y, minScale, maxScale);
            currentScale.z = Mathf.Clamp(currentScale.z, minScale, maxScale);

            // 应用缩放
            transform.localScale = currentScale;

            Debug.Log($"当前缩放: {currentScale}");
        }
    }
}
