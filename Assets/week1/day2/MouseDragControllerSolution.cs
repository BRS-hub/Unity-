using UnityEngine;

public class MouseDragControllerSolution : MonoBehaviour
{
    public float maxDragDistance = 100f;
    public PlaneType dragPlaneType = PlaneType.XZ;

    [Header("调试")]
    public bool showDebug = true;
    public Color highlightColor = Color.yellow;

    private Transform selectedObject = null;
    private Vector3 offset = Vector3.zero;
    private Plane dragPlane;
    private Material originalMaterial;
    private Material highlightMaterial;

    public enum PlaneType { XY, XZ, YZ }

    void Start()
    {
        // 创建高亮材质
        highlightMaterial = new Material(Shader.Find("Standard"));
        highlightMaterial.color = highlightColor;
    }

    void Update()
    {
        // 鼠标点击选择
        if (Input.GetMouseButtonDown(1))
        {
            TrySelectObject();
        }

        // 鼠标拖拽
        if (Input.GetMouseButton(1) && selectedObject != null)
        {
            DragSelectedObject();
        }

        // 鼠标释放
        if (Input.GetMouseButtonUp(1) && selectedObject != null)
        {
            DeselectObject();
        }
    }

    void TrySelectObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 射线检测
        if (Physics.Raycast(ray, out hit, maxDragDistance))
        {
            selectedObject = hit.transform;

            // 计算偏移量（物体中心到点击点的向量）
            offset = selectedObject.position - hit.point;

            // 设置拖拽平面
            SetupDragPlane(hit.point);

            // 高亮显示
            HighlightObject(true);

            Debug.Log($"选中物体: {selectedObject.name}");
        }
    }

    void SetupDragPlane(Vector3 point)
    {
        Vector3 normal = Vector3.up;
        switch (dragPlaneType)
        {
            case PlaneType.XY: normal = Vector3.forward; break; // Z轴为法线
            case PlaneType.XZ: normal = Vector3.up; break;      // Y轴为法线
            case PlaneType.YZ: normal = Vector3.right; break;   // X轴为法线
        }

        dragPlane = new Plane(normal, point);
    }

    void DragSelectedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter;

        if (dragPlane.Raycast(ray, out enter))
        {
            Vector3 targetPoint = ray.GetPoint(enter);
            // 应用偏移，保持点击点的相对位置
            selectedObject.position = targetPoint + offset;

            // 调试绘制
            if (showDebug)
            {
                Debug.DrawLine(ray.origin, targetPoint, Color.red);
                Debug.DrawLine(targetPoint, selectedObject.position, Color.blue);
            }
        }
    }

    void DeselectObject()
    {
        // 取消高亮
        HighlightObject(false);

        Debug.Log($"释放物体: {selectedObject.name}");
        selectedObject = null;
    }

    void HighlightObject(bool enable)
    {
        if (selectedObject == null) return;

        Renderer renderer = selectedObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (enable)
            {
                originalMaterial = renderer.material;
                renderer.material = highlightMaterial;
            }
            else
            {
                renderer.material = originalMaterial;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;

        // 绘制拖拽平面（简化表示）
        if (selectedObject != null)
        {
            Gizmos.color = Color.green;
            Vector3 planeCenter = selectedObject.position - offset;
            Vector3 planeSize = new Vector3(2, 0.01f, 2);

            // 根据平面类型调整显示
            switch (dragPlaneType)
            {
                case PlaneType.XY:
                    Gizmos.DrawCube(planeCenter, planeSize);
                    break;
                case PlaneType.XZ:
                    Gizmos.DrawCube(planeCenter, new Vector3(planeSize.x, planeSize.z, planeSize.y));
                    break;
                case PlaneType.YZ:
                    Gizmos.DrawCube(planeCenter, new Vector3(planeSize.z, planeSize.x, planeSize.y));
                    break;
            }
        }
    }
}