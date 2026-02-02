using UnityEngine;

public class SpringPhysicsSolution : MonoBehaviour
{
    public Transform anchor;
    public float stiffness = 10.0f;
    public float damping = 0.5f;
    public float mass = 1.0f;

    [Header("初始状态")]
    public Vector3 initialOffset = new Vector3(0, -2, 0);

    [Header("可视化")]
    public bool drawSpring = true;
    public Color springColor = Color.green;

    // 物理状态
    private Vector3 velocity = Vector3.zero;
    private Vector3 acceleration = Vector3.zero;

    // 线渲染器
    private LineRenderer lineRenderer;

    void Start()
    {
        if (anchor == null)
        {
            Debug.LogError("SpringPhysics: 请设置弹簧锚点！");
            enabled = false;
            return;
        }

        // 设置初始拉伸位置
        transform.position = anchor.position + initialOffset;

        // 创建线渲染器
        CreateLineRenderer();
    }

    void FixedUpdate()
    {
        if (anchor == null) return;

        // 1. 计算弹簧力（胡克定律）
        Vector3 springForce = -stiffness * (transform.position - anchor.position);

        // 2. 计算阻尼力
        Vector3 dampingForce = -damping * velocity;

        // 3. 计算重力
        Vector3 gravityForce = mass * Physics.gravity;

        // 4. 合力
        Vector3 totalForce = springForce + dampingForce + gravityForce;

        // 5. 计算加速度 (a = F/m)
        acceleration = totalForce / mass;

        // 6. 数值积分更新速度
        velocity += acceleration * Time.fixedDeltaTime;

        // 7. 更新位置
        transform.position += velocity * Time.fixedDeltaTime;

        // 8. 更新弹簧可视化
        UpdateSpringVisual();
    }

    void CreateLineRenderer()
    {
        GameObject lineObj = new GameObject("SpringLine");
        lineObj.transform.SetParent(transform);

        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = springColor;
        lineRenderer.endColor = springColor;
    }

    void UpdateSpringVisual()
    {
        if (lineRenderer != null && anchor != null)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, anchor.position);
        }
    }

    void OnDrawGizmos()
    {
        if (!drawSpring || anchor == null) return;

        Gizmos.color = springColor;
        Gizmos.DrawLine(transform.position, anchor.position);

        // 绘制弹簧线圈（简化）
        Vector3 direction = (anchor.position - transform.position).normalized;
        float length = Vector3.Distance(transform.position, anchor.position);
        int coils = Mathf.FloorToInt(length * 2);

        for (int i = 0; i < coils; i++)
        {
            float t = i / (float)coils;
            Vector3 point = Vector3.Lerp(transform.position, anchor.position, t);
            Vector3 perp = Vector3.Cross(direction, Vector3.up).normalized * 0.1f;
            if (i % 2 == 0) perp = -perp;

            Gizmos.DrawSphere(point + perp, 0.03f);
        }
    }
}