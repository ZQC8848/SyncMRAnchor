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
        // ɨ�赱ǰ����ͷ֡�еĶ�ά��
        var qrResults = await scanner.ScanFrameAsync() ?? Array.Empty<QrCodeResult>();

        // ��������ɨ�赽�Ķ�ά����
        foreach (var qrResult in qrResults)
        {
            // ����ά��Ľǵ��Ƿ���������ٰ���4����
            if (qrResult?.corners == null || qrResult.corners.Length < 4)
            {
                continue; // ���������������������ǰ��ά��
            }

            // ��ȡ��ά��ǵ������
            var count = qrResult.corners.Length;

            // ����ά��Ľǵ�����ת��ΪUV���꣨��Χ[0, 1]��
            var uvs = new Vector2[count];
            for (var i = 0; i < count; i++)
            {
                uvs[i] = new Vector2(qrResult.corners[i].x, qrResult.corners[i].y);
            }

            // �����ά�����ĵ��UV���꣨���нǵ�UV�����ƽ��ֵ��
            var centerUV = Vector2.zero;
            foreach (var uv in uvs)
            {
                centerUV += uv;
            }
            centerUV /= count;

            // ��ȡ����ͷ���ڲΣ������ֱ��ʵ���Ϣ��
            var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(_passthroughCameraEye);

            // �����ĵ��UV����ת��Ϊ��������
            var centerPixel = new Vector2Int(
                Mathf.RoundToInt(centerUV.x * intrinsics.Resolution.x),
                Mathf.RoundToInt(centerUV.y * intrinsics.Resolution.y)
            );

            // ����������ת��Ϊ3D�ռ��е�����
            var centerRay = PassthroughCameraUtils.ScreenPointToRayInWorld(_passthroughCameraEye, centerPixel);

            // �������߼�⣬��ȡ�����뻷���Ľ���
            if (!envRaycastManager || !envRaycastManager.Raycast(centerRay, out var hitInfo))
            {
                continue; // ������߼��ʧ�ܣ�������ǰ��ά��
            }

            // ��ȡ���߼��Ľ��㣬����ά�����ĵ��3Dλ��
            //ͨ�����߼��õ���centerֻ��һ������ֵ�������ܵ�����������״��Ӱ�졣
            //�����ά�����ڵı��治����ȫƽ̹�ģ��������߼��ľ������ޣ����center���ܻ���ƫ��
            var center = hitInfo.point;

            // ����������㵽����ľ���
            var distance = Vector3.Distance(centerRay.origin, hitInfo.point);

            // �����ά��ǵ��3Dλ��
            var tempCorners = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                // ��ÿ���ǵ��UV����ת��Ϊ��������
                var pixelCoord = new Vector2Int(
                    Mathf.RoundToInt(uvs[i].x * intrinsics.Resolution.x),
                    Mathf.RoundToInt(uvs[i].y * intrinsics.Resolution.y)
                );

                // ����������ת��Ϊ3D�ռ��е�����
                var r = PassthroughCameraUtils.ScreenPointToRayInWorld(_passthroughCameraEye, pixelCoord);

                // ���ݾ������ÿ���ǵ��3Dλ��
                tempCorners[i] = r.origin + r.direction * distance;
            }

            // �����ά��ƽ��ķ��ߺ���ת
            var up = (tempCorners[1] - tempCorners[0]).normalized; // �Ϸ���
            var right = (tempCorners[2] - tempCorners[1]).normalized; // �ҷ���
            var normal = -Vector3.Cross(up, right).normalized; // ���߷���

            // ����һ��ƽ�棬���ں�����������ƽ�潻�����
            var qrPlane = new Plane(normal, center);

            // �����ά��ǵ������3Dλ��
            var worldCorners = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                // ��ÿ���ǵ��UV����ת��Ϊ��������
                var pixelCoord = new Vector2Int(
                    Mathf.RoundToInt(uvs[i].x * intrinsics.Resolution.x),
                    Mathf.RoundToInt(uvs[i].y * intrinsics.Resolution.y)
                );

                // ����������ת��Ϊ3D�ռ��е�����
                var r = PassthroughCameraUtils.ScreenPointToRayInWorld(_passthroughCameraEye, pixelCoord);

                // ʹ��ƽ���������󽻵㣬�õ���ά��ǵ������3Dλ��
                if (qrPlane.Raycast(r, out var enter))
                {
                    worldCorners[i] = r.GetPoint(enter);
                }
                else
                {
                    // ���������ƽ��û�н��㣬��ʹ��֮ǰ�������ʱ�ǵ�λ��
                    worldCorners[i] = tempCorners[i];
                }
            }

            // �����ά�����ĵ������3Dλ��
            //��ά��Ľǵ㣨worldCorners����ͨ��ƽ�������ߵĽ������õ��ģ����Ӿ�ȷ��
            //ͨ���ǵ����¼������ĵ㣬����������һ���߼�����ȷ����ά���ǵ�λ�ø���׼ȷ
            center = Vector3.zero;
            foreach (var corner in worldCorners)
            {
                center += corner;
            }
            center /= count;

            // ���¼����ά��ƽ��ķ��ߺ���ת
            up = (worldCorners[1] - worldCorners[0]).normalized;
            right = (worldCorners[2] - worldCorners[1]).normalized;
            normal = -Vector3.Cross(up, right).normalized;

            // �����ά�����ת����Ԫ����
            var poseRot = Quaternion.LookRotation(normal, up);

            // �����ά��Ŀ�Ⱥ͸߶�
            var width = Vector3.Distance(worldCorners[0], worldCorners[1]);
            var height = Vector3.Distance(worldCorners[0], worldCorners[3]);

            // �����������Ӽ������յ�����
            var scaleFactor = 1.5f;
            var scale = new Vector3(width * scaleFactor, height * scaleFactor, 1f);

            // ���»򴴽���ά����
            if (_activeMarkers.TryGetValue(qrResult.text, out var marker))
            {
                // �������Ѿ����ڣ��������λ�á���ת������
                marker.UpdateMarker(center, poseRot, scale, qrResult.text);
            }
            else
            {
                // �����ǲ����ڣ���Ӷ�����л�ȡһ����Ƕ���
                var markerGo = MarkerPool.Instance.GetMarker();
                if (!markerGo)
                {
                    continue; // �����ȡʧ�ܣ�������ǰ��ά��
                }

                // ��ȡ��ǿ��������
                marker = markerGo.GetComponent<MarkerController>();
                if (!marker)
                {
                    continue; // �����������ڣ�������ǰ��ά��
                }

                // ��ʼ����ǵ�λ�á���ת������
                marker.UpdateMarker(center, poseRot, scale, qrResult.text);

                // ���´����ı����ӵ������ֵ���
                _activeMarkers[qrResult.text] = marker;
            }
        }

        // �����ٻ�ı��
        var keysToRemove = new List<string>();
        foreach (var kvp in _activeMarkers)
        {
            // ����Ƕ����Ƿ��ڻ״̬
            if (!kvp.Value.gameObject.activeSelf)
                keysToRemove.Add(kvp.Key); // �����Ǳ����ã�����ӵ����Ƴ��б�
        }

        // �������Ƴ��б��ӻ����ֵ����Ƴ����ٻ�ı��
        foreach (var key in keysToRemove)
        {
            _activeMarkers.Remove(key);
        }
    }
#endif
}
