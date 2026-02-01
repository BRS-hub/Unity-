using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionExample : MonoBehaviour
{
    public Transform lookTarget;

    void Update()
    {
        // 1. 创建旋转（避免万向节死锁）
        Quaternion rotation = Quaternion.Euler(30, 45, 0);
        transform.rotation = rotation;

        // 2. 朝向目标（常用于敌人/炮塔）
        if (lookTarget != null)
        {
            Vector3 direction = lookTarget.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 3. 平滑插值旋转
            float rotationSpeed = 2.0f;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // 4. 向量旋转
        Vector3 originalDirection = Vector3.forward;
        Vector3 rotatedDirection = rotation * originalDirection;
    }
}
