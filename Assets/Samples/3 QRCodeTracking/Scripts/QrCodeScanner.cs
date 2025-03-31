using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassthroughCameraSamples;
using UnityEngine;
using UnityEngine.Rendering;
#if ZXING_ENABLED
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Multi;
#endif

// ��ά����ģʽö��
public enum QrCodeDetectionMode
{
    Single,   // ������ά��ģʽ
    Multiple  // �����ά��ģʽ
}

// ��ά������
[Serializable]
public class QrCodeResult
{
    public string text;      // ��ά��Ľ����ı�
    public Vector3[] corners; // ��ά����ĸ��ǵ�����
}

public class QrCodeScanner : MonoBehaviour
{
#if ZXING_ENABLED
    [SerializeField] private WebCamTextureManager camHelper;  // ����ͷ������
    [SerializeField] private int sampleFactor = 2;           // �²������ӣ����ڼ��ٴ���
    [SerializeField] private QrCodeDetectionMode detectionMode = QrCodeDetectionMode.Single; // ��ά����ģʽ
    [SerializeField] private ComputeShader downsampleShader; // ����ͼ���²����ļ�����ɫ��

    private RenderTexture _downsampledTexture;              // �²����������
    private Texture2D _webcamTextureCache;                  // ���������ͷ����
    private QRCodeReader _qrReader;                         // ZXing���еĶ�ά�������
    private bool _isScanning;                               // ����Ƿ�����ɨ��

    // ������ɫ����Ҫ�õ���ID
    private static readonly int Input1 = Shader.PropertyToID("_Input");
    private static readonly int Output = Shader.PropertyToID("_Output");
    private static readonly int InputWidth = Shader.PropertyToID("_InputWidth");
    private static readonly int InputHeight = Shader.PropertyToID("_InputHeight");
    private static readonly int OutputWidth = Shader.PropertyToID("_OutputWidth");
    private static readonly int OutputHeight = Shader.PropertyToID("_OutputHeight");

    // ��ʼ����ά�������
    private void Awake()
    {
        _qrReader = new QRCodeReader();  // ��ʼ��QRCodeReader������
    }

    // ����ʱ�ͷ���Դ
    private void OnDestroy()
    {
        if (_downsampledTexture != null)
        {
            _downsampledTexture.Release();
            Destroy(_downsampledTexture);  // �ͷ��²�������
        }
        if (_webcamTextureCache != null)
        {
            Destroy(_webcamTextureCache);  // ���ٻ��������ͷ����
        }
    }

    // ��ȡ�򴴽�һ��������
    private Texture2D GetOrCreateTexture(int width, int height)
    {
        if (_webcamTextureCache && _webcamTextureCache.width == width && _webcamTextureCache.height == height)
        {
            return _webcamTextureCache;  // �����������ĳߴ��Ҫ���һ�£�ֱ�ӷ��ػ��������
        }

        if (_webcamTextureCache)
        {
            Destroy(_webcamTextureCache);  // ���پɵ�������
        }

        _webcamTextureCache = new Texture2D(width, height, TextureFormat.RGBA32, false);  // �����µ�����
        return _webcamTextureCache;
    }

    // �첽ɨ�赱ǰ֡�Ķ�ά��
    public async Task<QrCodeResult[]> ScanFrameAsync()
    {
        if (_isScanning)
            return null;  // ����Ѿ���ɨ�裬ֱ�ӷ���

        _isScanning = true;
        try
        {
            if (!camHelper)
            {
                Debug.LogWarning("[QRCodeScanner] Camera helper is not assigned.");
                return null;
            }

            var webCamTex = camHelper.WebCamTexture;  // ��ȡ����ͷ��WebCamTexture
            while (!webCamTex || !webCamTex.isPlaying)
            {
                await Task.Delay(16);  // �ȴ�����ͷ��ʼ��
                webCamTex = camHelper.WebCamTexture;
            }

            var texture = GetOrCreateTexture(webCamTex.width, webCamTex.height);  // �������ȡ������
            texture.SetPixels(webCamTex.GetPixels());  // ��ȡ����ͷ����������
            texture.Apply();  // Ӧ���������

            var originalWidth = texture.width;
            var originalHeight = texture.height;
            var targetWidth = Mathf.Max(1, originalWidth / sampleFactor);  // �����²������Ӽ���Ŀ����
            var targetHeight = Mathf.Max(1, originalHeight / sampleFactor);  // �����²������Ӽ���Ŀ��߶�

            // ����²����������ߴ粻ƥ�䣬�򴴽��µ��²�������
            if (!_downsampledTexture || _downsampledTexture.width != targetWidth || _downsampledTexture.height != targetHeight)
            {
                if (_downsampledTexture)
                {
                    _downsampledTexture.Release();
                }

                _downsampledTexture = new RenderTexture(targetWidth, targetHeight, 0, RenderTextureFormat.R8)
                {
                    enableRandomWrite = true  // �������д��
                };

                _downsampledTexture.Create();  // �����²�������
            }

            // ���ü�����ɫ������
            var kernel = downsampleShader.FindKernel("CSMain");
            downsampleShader.SetTexture(kernel, Input1, texture);
            downsampleShader.SetTexture(kernel, Output, _downsampledTexture);
            downsampleShader.SetInt(InputWidth, originalWidth);
            downsampleShader.SetInt(InputHeight, originalHeight);
            downsampleShader.SetInt(OutputWidth, targetWidth);
            downsampleShader.SetInt(OutputHeight, targetHeight);

            var threadGroupsX = Mathf.CeilToInt(targetWidth / 8f);  // ������ɫ�����߳���X���С
            var threadGroupsY = Mathf.CeilToInt(targetHeight / 8f);  // ������ɫ�����߳���Y���С
            downsampleShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);  // ִ�м�����ɫ��

            // �첽��ȡ�²����������
            var grayBytes = await ReadPixelsAsync(_downsampledTexture);
            var luminanceSource = new RGBLuminanceSource(grayBytes, targetWidth, targetHeight, RGBLuminanceSource.BitmapFormat.Gray8);
            var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));

            // ��ʼ��ά�����
            return await Task.Run(() =>
            {
                try
                {
                    if (detectionMode == QrCodeDetectionMode.Single)
                    {
                        // ������ά����ģʽ
                        var decodeResult = _qrReader.decode(binaryBitmap);
                        if (decodeResult != null)
                            return new[] { ProcessDecodeResult(decodeResult, targetWidth, targetHeight) };
                    }
                    else
                    {
                        // �����ά����ģʽ
                        var multiReader = new GenericMultipleBarcodeReader(_qrReader);
                        var decodeResults = multiReader.decodeMultiple(binaryBitmap);
                        if (decodeResults != null)
                        {
                            var results = new List<QrCodeResult>();
                            foreach (var decodeResult in decodeResults)
                            {
                                results.Add(ProcessDecodeResult(decodeResult, targetWidth, targetHeight));
                            }

                            return results.ToArray();  // ���ؽ�����
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[QRCodeScanner] Error decoding QR code(s): {ex.Message}");
                }
                return null;
            });
        }
        finally
        {
            _isScanning = false;  // ����������ָ�ɨ��״̬
        }
    }

    // ���������������ά��Ľǵ����������ת��ΪUV����
    private QrCodeResult ProcessDecodeResult(Result decodeResult, int targetWidth, int targetHeight)
    {
        var points = decodeResult.ResultPoints;  // ��ȡ��ά��Ľǵ�
        var uvCorners = new Vector3[points.Length];
        for (var i = 0; i < points.Length; i++)
        {
            // ���ǵ����������ת��ΪUV����
            uvCorners[i] = new Vector3(points[i].X / targetWidth, points[i].Y / targetHeight, 0);
        }

        return new QrCodeResult
        {
            text = decodeResult.Text,  // ���ؽ����ı�
            corners = uvCorners        // ���ض�ά��ǵ��UV����
        };
    }

    // �첽��ȡRenderTexture�е���������
    private Task<byte[]> ReadPixelsAsync(RenderTexture rt)
    {
        var tcs = new TaskCompletionSource<byte[]>();

        // �첽GPU��ȡ��������
        AsyncGPUReadback.Request(rt, 0, TextureFormat.R8, request =>
        {
            if (request.hasError)
            {
                tcs.SetException(new Exception("GPU readback error."));  // �����ȡ���������쳣
            }
            else
            {
                tcs.SetResult(request.GetData<byte>().ToArray());  // �ɹ���ȡ���ݣ���������
            }
        });
        return tcs.Task;  // �����첽����
    }
#endif
}
