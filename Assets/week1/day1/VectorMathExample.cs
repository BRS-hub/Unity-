using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorMathExample : MonoBehaviour
{
    void Start()
    {
        // 创建向量
        Vector3 pointA = new Vector3(1, 2, 3);
        Vector3 pointB = new Vector3(4, 6, 8);

        // 1. 计算两点间距离
        float distance = Vector3.Distance(pointA, pointB);
        Debug.Log($"两点距离: {distance}");

        // 2. 向量加法（位移叠加）
        Vector3 displacement = pointB - pointA;
        Debug.Log($"位移向量: {displacement}");

        // 3. 向量归一化（获取方向）
        Vector3 direction = displacement.normalized;
        Debug.Log($"方向向量: {direction}");

        // 4. 向量点积（判断夹角）
        Vector3 forward = transform.forward;
        float dotProduct = Vector3.Dot(forward, direction);

        if (dotProduct > 0.7f)
            Debug.Log("目标在正前方");
        else if (dotProduct < -0.7f)
            Debug.Log("目标在正后方");
        else
            Debug.Log($"目标夹角: {Vector3.Angle(forward, direction)}°");

        // 5. 向量叉积（计算法向量）
        Vector3 cross = Vector3.Cross(forward, direction);
        Debug.Log($"叉积结果（垂直向量）: {cross}");
    }
}
