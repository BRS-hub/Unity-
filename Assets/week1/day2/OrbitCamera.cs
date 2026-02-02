using UnityEngine;

public class OrbitCameraSolution : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 2.0f;
    public float zoomSpeed = 2.0f;

    [Header("旋转限制")]
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;

    [Header("距离限制")]
    public float minRadius = 2.0f;
    public float maxRadius = 20.0f;

    // 当前状态
    private float currentAngleY = 0f;
    private float currentAngleX = 30f;
    private float currentRadius = 5.0f;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("OrbitCamera: 请设置目标物体！");
            enabled = false;
            return;
        }
        UpdateCameraPosition();
    }

    void Update()
    {
        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        // 鼠标左键拖拽旋转
        if (Input.GetMouseButton(0))
        {
            currentAngleY += Input.GetAxis("Mouse X") * rotationSpeed;
            currentAngleX -= Input.GetAxis("Mouse Y") * rotationSpeed;
            currentAngleX = Mathf.Clamp(currentAngleX, minVerticalAngle, maxVerticalAngle);
        }

        // 滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentRadius -= scroll * zoomSpeed;
            currentRadius = Mathf.Clamp(currentRadius, minRadius, maxRadius);
        }
    }

    void UpdateCameraPosition()
    {
        // 使用四元数计算相机位置
        Quaternion rotation = Quaternion.Euler(currentAngleX, currentAngleY, 0);
        Vector3 offset = rotation * Vector3.forward * currentRadius;

        // 更新相机位置和朝向
        transform.position = target.position - offset;
        transform.LookAt(target);

        // 调试输出
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"相机状态: 角度({currentAngleX:F1}, {currentAngleY:F1}), 距离{currentRadius:F1}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position, 0.5f);
        }
    }
}