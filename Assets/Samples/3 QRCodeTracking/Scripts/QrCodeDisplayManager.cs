using System;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR;
using PassthroughCameraSamples;

public class QrCodeDisplayManager : MonoBehaviour
{
#if ZXING_ENABLED
    [SerializeField] private QrCodeScanner scanner;
    [SerializeField] private EnvironmentRaycastManager envRaycastManager;
    [SerializeField] private WebCamTextureManager passthroughCameraManager;

    private readonly Dictionary<string, MarkerController> _activeMarkers = new();
    private PassthroughCameraEye _passthroughCameraEye;

    private void Awake()
    {
        _passthroughCameraEye = passthroughCameraManager.eye;
    }

    private void Update()
    {
        UpdateMarkers();
    }

    private async void UpdateMarkers()
    {
        // 扫描当前摄像头帧中的二维码
        var qrResults = await scanner.ScanFrameAsync() ?? Array.Empty<QrCodeResult>();

        // 遍历所有扫描到的二维码结果
        foreach (var qrResult in qrResults)
        {
            // 检查二维码的角点是否存在且至少包含4个点
            if (qrResult?.corners == null || qrResult.corners.Length < 4)
            {
                continue; // 如果不符合条件，跳过当前二维码
            }

            // 获取二维码角点的数量
            var count = qrResult.corners.Length;

            // 将二维码的角点坐标转换为UV坐标（范围[0, 1]）
            var uvs = new Vector2[count];
            for (var i = 0; i < count; i++)
            {
                uvs[i] = new Vector2(qrResult.corners[i].x, qrResult.corners[i].y);
            }

            // 计算二维码中心点的UV坐标（所有角点UV坐标的平均值）
            var centerUV = Vector2.zero;
            foreach (var uv in uvs)
            {
                centerUV += uv;
            }
            centerUV /= count;

            // 获取摄像头的内参（包括分辨率等信息）
            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(_passthroughCameraEye);

            // 将中心点的UV坐标转换为像素坐标
            var centerPixel = new Vector2Int(
                Mathf.RoundToInt(centerUV.x * intrinsics.Resolution.x),
                Mathf.RoundToInt(centerUV.y * intrinsics.Resolution.y)
            );

            // 将像素坐标转换为3D空间中的射线
            var centerRay = PassthroughCameraUtils.ScreenPointToRayInWorld(_passthroughCameraEye, centerPixel);

            // 进行射线检测，获取射线与环境的交点
            if (!envRaycastManager || !envRaycastManager.Raycast(centerRay, out var hitInfo))
            {
                continue; // 如果射线检测失败，跳过当前二维码
            }

            // 获取射线检测的交点，即二维码中心点的3D位置
            //通过射线检测得到的center只是一个近似值，可能受到环境几何形状的影响。
            //如果二维码所在的表面不是完全平坦的，或者射线检测的精度有限，这个center可能会有偏差
            var center = hitInfo.point;

            // 计算射线起点到交点的距离
            var distance = Vector3.Distance(centerRay.origin, hitInfo.point);

            // 计算二维码角点的3D位置
            var tempCorners = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                // 将每个角点的UV坐标转换为像素坐标
                var pixelCoord = new Vector2Int(
                    Mathf.RoundToInt(uvs[i].x * intrinsics.Resolution.x),
                    Mathf.RoundToInt(uvs[i].y * intrinsics.Resolution.y)
                );

                // 将像素坐标转换为3D空间中的射线
                var r = PassthroughCameraUtils.ScreenPointToRayInWorld(_passthroughCameraEye, pixelCoord);

                // 根据距离计算每个角点的3D位置
                tempCorners[i] = r.origin + r.direction * distance;
            }

            // 计算二维码平面的法线和旋转
            var up = (tempCorners[1] - tempCorners[0]).normalized; // 上方向
            var right = (tempCorners[2] - tempCorners[1]).normalized; // 右方向
            var normal = -Vector3.Cross(up, right).normalized; // 法线方向

            // 创建一个平面，用于后续的射线与平面交点计算
            var qrPlane = new Plane(normal, center);

            // 计算二维码角点的最终3D位置
            var worldCorners = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                // 将每个角点的UV坐标转换为像素坐标
                var pixelCoord = new Vector2Int(
                    Mathf.RoundToInt(uvs[i].x * intrinsics.Resolution.x),
                    Mathf.RoundToInt(uvs[i].y * intrinsics.Resolution.y)
                );

                // 将像素坐标转换为3D空间中的射线
                var r = PassthroughCameraUtils.ScreenPointToRayInWorld(_passthroughCameraEye, pixelCoord);

                // 使用平面与射线求交点，得到二维码角点的最终3D位置
                if (qrPlane.Raycast(r, out var enter))
                {
                    worldCorners[i] = r.GetPoint(enter);
                }
                else
                {
                    // 如果射线与平面没有交点，则使用之前计算的临时角点位置
                    worldCorners[i] = tempCorners[i];
                }
            }

            // 计算二维码中心点的最终3D位置
            //二维码的角点（worldCorners）是通过平面与射线的交点计算得到的，更加精确。
            //通过角点重新计算中心点，可以消除单一射线检测的误差，确保二维码标记的位置更加准确
            center = Vector3.zero;
            foreach (var corner in worldCorners)
            {
                center += corner;
            }
            center /= count;

            // 重新计算二维码平面的法线和旋转
            up = (worldCorners[1] - worldCorners[0]).normalized;
            right = (worldCorners[2] - worldCorners[1]).normalized;
            normal = -Vector3.Cross(up, right).normalized;

            // 计算二维码的旋转（四元数）
            var poseRot = Quaternion.LookRotation(normal, up);

            // 计算二维码的宽度和高度
            var width = Vector3.Distance(worldCorners[0], worldCorners[1]);
            var height = Vector3.Distance(worldCorners[0], worldCorners[3]);

            // 根据缩放因子计算最终的缩放
            var scaleFactor = 1.5f;
            var scale = new Vector3(width * scaleFactor, height * scaleFactor, 1f);

            // 更新或创建二维码标记
            if (_activeMarkers.TryGetValue(qrResult.text, out var marker))
            {
                // 如果标记已经存在，则更新其位置、旋转和缩放
                marker.UpdateMarker(center, poseRot, scale, qrResult.text);
            }
            else
            {
                // 如果标记不存在，则从对象池中获取一个标记对象
                var markerGo = MarkerPool.Instance.GetMarker();
                if (!markerGo)
                {
                    continue; // 如果获取失败，跳过当前二维码
                }

                // 获取标记控制器组件
                marker = markerGo.GetComponent<MarkerController>();
                if (!marker)
                {
                    continue; // 如果组件不存在，跳过当前二维码
                }

                // 初始化标记的位置、旋转和缩放
                marker.UpdateMarker(center, poseRot, scale, qrResult.text);

                // 将新创建的标记添加到活动标记字典中
                _activeMarkers[qrResult.text] = marker;
            }
        }

        // 清理不再活动的标记
        var keysToRemove = new List<string>();
        foreach (var kvp in _activeMarkers)
        {
            // 检查标记对象是否处于活动状态
            if (!kvp.Value.gameObject.activeSelf)
                keysToRemove.Add(kvp.Key); // 如果标记被禁用，则添加到待移除列表
        }

        // 遍历待移除列表，从活动标记字典中移除不再活动的标记
        foreach (var key in keysToRemove)
        {
            _activeMarkers.Remove(key);
        }
    }
#endif
}
