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

// 二维码检测模式枚举
public enum QrCodeDetectionMode
{
    Single,   // 单个二维码模式
    Multiple  // 多个二维码模式
}

// 二维码结果类
[Serializable]
public class QrCodeResult
{
    public string text;      // 二维码的解码文本
    public Vector3[] corners; // 二维码的四个角点坐标
}

public class QrCodeScanner : MonoBehaviour
{
#if ZXING_ENABLED
    [SerializeField] private WebCamTextureManager camHelper;  // 摄像头管理器
    [SerializeField] private int sampleFactor = 2;           // 下采样因子，用于加速处理
    [SerializeField] private QrCodeDetectionMode detectionMode = QrCodeDetectionMode.Single; // 二维码检测模式
    [SerializeField] private ComputeShader downsampleShader; // 用于图像下采样的计算着色器

    private RenderTexture _downsampledTexture;              // 下采样后的纹理
    private Texture2D _webcamTextureCache;                  // 缓存的摄像头纹理
    private QRCodeReader _qrReader;                         // ZXing库中的二维码解码器
    private bool _isScanning;                               // 标记是否正在扫描

    // 计算着色器需要用到的ID
    private static readonly int Input1 = Shader.PropertyToID("_Input");
    private static readonly int Output = Shader.PropertyToID("_Output");
    private static readonly int InputWidth = Shader.PropertyToID("_InputWidth");
    private static readonly int InputHeight = Shader.PropertyToID("_InputHeight");
    private static readonly int OutputWidth = Shader.PropertyToID("_OutputWidth");
    private static readonly int OutputHeight = Shader.PropertyToID("_OutputHeight");

    // 初始化二维码解码器
    private void Awake()
    {
        _qrReader = new QRCodeReader();  // 初始化QRCodeReader解码器
    }

    // 销毁时释放资源
    private void OnDestroy()
    {
        if (_downsampledTexture != null)
        {
            _downsampledTexture.Release();
            Destroy(_downsampledTexture);  // 释放下采样纹理
        }
        if (_webcamTextureCache != null)
        {
            Destroy(_webcamTextureCache);  // 销毁缓存的摄像头纹理
        }
    }

    // 获取或创建一个纹理缓存
    private Texture2D GetOrCreateTexture(int width, int height)
    {
        if (_webcamTextureCache && _webcamTextureCache.width == width && _webcamTextureCache.height == height)
        {
            return _webcamTextureCache;  // 如果缓存纹理的尺寸和要求的一致，直接返回缓存的纹理
        }

        if (_webcamTextureCache)
        {
            Destroy(_webcamTextureCache);  // 销毁旧的纹理缓存
        }

        _webcamTextureCache = new Texture2D(width, height, TextureFormat.RGBA32, false);  // 创建新的纹理
        return _webcamTextureCache;
    }

    // 异步扫描当前帧的二维码
    public async Task<QrCodeResult[]> ScanFrameAsync()
    {
        if (_isScanning)
            return null;  // 如果已经在扫描，直接返回

        _isScanning = true;
        try
        {
            if (!camHelper)
            {
                Debug.LogWarning("[QRCodeScanner] Camera helper is not assigned.");
                return null;
            }

            var webCamTex = camHelper.WebCamTexture;  // 获取摄像头的WebCamTexture
            while (!webCamTex || !webCamTex.isPlaying)
            {
                await Task.Delay(16);  // 等待摄像头初始化
                webCamTex = camHelper.WebCamTexture;
            }

            var texture = GetOrCreateTexture(webCamTex.width, webCamTex.height);  // 创建或获取纹理缓存
            texture.SetPixels(webCamTex.GetPixels());  // 获取摄像头的像素数据
            texture.Apply();  // 应用纹理更新

            var originalWidth = texture.width;
            var originalHeight = texture.height;
            var targetWidth = Mathf.Max(1, originalWidth / sampleFactor);  // 根据下采样因子计算目标宽度
            var targetHeight = Mathf.Max(1, originalHeight / sampleFactor);  // 根据下采样因子计算目标高度

            // 如果下采样后的纹理尺寸不匹配，则创建新的下采样纹理
            if (!_downsampledTexture || _downsampledTexture.width != targetWidth || _downsampledTexture.height != targetHeight)
            {
                if (_downsampledTexture)
                {
                    _downsampledTexture.Release();
                }

                _downsampledTexture = new RenderTexture(targetWidth, targetHeight, 0, RenderTextureFormat.R8)
                {
                    enableRandomWrite = true  // 启用随机写入
                };

                _downsampledTexture.Create();  // 创建下采样纹理
            }

            // 设置计算着色器参数
            var kernel = downsampleShader.FindKernel("CSMain");
            downsampleShader.SetTexture(kernel, Input1, texture);
            downsampleShader.SetTexture(kernel, Output, _downsampledTexture);
            downsampleShader.SetInt(InputWidth, originalWidth);
            downsampleShader.SetInt(InputHeight, originalHeight);
            downsampleShader.SetInt(OutputWidth, targetWidth);
            downsampleShader.SetInt(OutputHeight, targetHeight);

            var threadGroupsX = Mathf.CeilToInt(targetWidth / 8f);  // 计算着色器的线程组X轴大小
            var threadGroupsY = Mathf.CeilToInt(targetHeight / 8f);  // 计算着色器的线程组Y轴大小
            downsampleShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);  // 执行计算着色器

            // 异步读取下采样后的纹理
            var grayBytes = await ReadPixelsAsync(_downsampledTexture);
            var luminanceSource = new RGBLuminanceSource(grayBytes, targetWidth, targetHeight, RGBLuminanceSource.BitmapFormat.Gray8);
            var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));

            // 开始二维码解码
            return await Task.Run(() =>
            {
                try
                {
                    if (detectionMode == QrCodeDetectionMode.Single)
                    {
                        // 单个二维码检测模式
                        var decodeResult = _qrReader.decode(binaryBitmap);
                        if (decodeResult != null)
                            return new[] { ProcessDecodeResult(decodeResult, targetWidth, targetHeight) };
                    }
                    else
                    {
                        // 多个二维码检测模式
                        var multiReader = new GenericMultipleBarcodeReader(_qrReader);
                        var decodeResults = multiReader.decodeMultiple(binaryBitmap);
                        if (decodeResults != null)
                        {
                            var results = new List<QrCodeResult>();
                            foreach (var decodeResult in decodeResults)
                            {
                                results.Add(ProcessDecodeResult(decodeResult, targetWidth, targetHeight));
                            }

                            return results.ToArray();  // 返回解码结果
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
            _isScanning = false;  // 解码结束，恢复扫描状态
        }
    }

    // 处理解码结果，将二维码的角点从像素坐标转换为UV坐标
    private QrCodeResult ProcessDecodeResult(Result decodeResult, int targetWidth, int targetHeight)
    {
        var points = decodeResult.ResultPoints;  // 获取二维码的角点
        var uvCorners = new Vector3[points.Length];
        for (var i = 0; i < points.Length; i++)
        {
            // 将角点的像素坐标转换为UV坐标
            uvCorners[i] = new Vector3(points[i].X / targetWidth, points[i].Y / targetHeight, 0);
        }

        return new QrCodeResult
        {
            text = decodeResult.Text,  // 返回解码文本
            corners = uvCorners        // 返回二维码角点的UV坐标
        };
    }

    // 异步读取RenderTexture中的像素数据
    private Task<byte[]> ReadPixelsAsync(RenderTexture rt)
    {
        var tcs = new TaskCompletionSource<byte[]>();

        // 异步GPU读取纹理数据
        AsyncGPUReadback.Request(rt, 0, TextureFormat.R8, request =>
        {
            if (request.hasError)
            {
                tcs.SetException(new Exception("GPU readback error."));  // 如果读取出错，返回异常
            }
            else
            {
                tcs.SetResult(request.GetData<byte>().ToArray());  // 成功读取数据，返回数据
            }
        });
        return tcs.Task;  // 返回异步任务
    }
#endif
}
