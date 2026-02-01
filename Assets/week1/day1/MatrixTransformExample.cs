using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixTransformExample : MonoBehaviour
{
    public GameObject targetObject;

    void Start()
    {
        // 1. 创建基础变换矩阵
        // 平移矩阵：移动(2, 3, 4)单位
        Matrix4x4 translation = Matrix4x4.Translate(new Vector3(2, 3, 4));

        // 旋转矩阵：绕Y轴旋转45°
        Matrix4x4 rotation = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));

        // 缩放矩阵：X,Y,Z各缩放2倍
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(2, 2, 2));

        // 2. 组合变换（注意顺序！）
        Matrix4x4 composite = translation * rotation * scale;

        // 3. 应用变换到点
        Vector3 originalPoint = new Vector3(1, 0, 0);
        Vector3 transformedPoint = composite.MultiplyPoint(originalPoint);
        Debug.Log($"变换后坐标: {transformedPoint}");

        // 4. Unity Transform组件底层原理
        // transform.position → 世界坐标
        // transform.localPosition → 局部坐标
        // transform.TransformPoint() → 局部转世界
        // transform.InverseTransformPoint() → 世界转局部
    }

    void Update()
    {
        // 5. 实际开发中的常用模式
        // 平滑移动：向量加法 + Time.deltaTime
        float speed = 5.0f;
        Vector3 direction = (targetObject.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}
