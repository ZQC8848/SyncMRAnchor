using System;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;  // 你可以通过Inspector指定主摄像头

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 获取UI面板与摄像头的方向向量
        Vector3 direction = mainCamera.transform.position - transform.position;

        // 将面板的旋转仅调整到水平方向
        direction.y = 0;  // 只影响x和z轴的旋转，y轴保持不变

        // 旋转面板朝向摄像头
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }
}