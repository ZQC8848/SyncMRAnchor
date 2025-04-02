using UnityEngine;
using OVR.Input;

public class OculusHandSpawner : MonoBehaviour
{
    [Header("手部配置")]
    [SerializeField] private OVRHand _rightHand; // 绑定右手OVRHand组件
    [SerializeField] private Transform _pinchPoint; // OculusHandPinchArrowBlended节点

    [Header("生成设置")]
    [SerializeField] private GameObject[] _prefabs; // 拖拽多个预制体到此处
    [SerializeField] private float _spawnCooldown = 0.5f; // 生成冷却时间

    private GameObject _currentObject;     // 当前生成的对象
    private bool _wasPinching;            // 上一帧捏合状态
    private float _lastSpawnTime;         // 上次生成时间
    private int _currentPrefabIndex;      // 当前选择的预制体索引

    public bool handInTheTree;//判断手是否在树里面

    void Start()
    {
        // 自动查找关键节点
        if (!_pinchPoint)
            _pinchPoint = GameObject.Find("OculusHandPinchArrowBlended").transform;
    }

    void Update()
    {
        HandlePinchGesture();
        HandleObjectPosition();
        HandlePrefabSelection();
    }

    // 手势检测逻辑
    void HandlePinchGesture()
    {
        bool isPinching = _rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // 仅在状态变化时触发
        if (isPinching != _wasPinching)
        {
            if (isPinching) TrySpawnPrefab();
            else ReleaseObject();
        }

        _wasPinching = isPinching;
    }

    // 尝试生成预制体
    void TrySpawnPrefab()
    {
        if (Time.time - _lastSpawnTime < _spawnCooldown) return;
        if (_prefabs.Length == 0 || _currentPrefabIndex >= _prefabs.Length) return;
        if (!handInTheTree) return;

        _currentObject = Instantiate(
            _prefabs[_currentPrefabIndex],
            _pinchPoint.position,
            _pinchPoint.rotation
        );

        // 初始化物理参数
        if (_currentObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        _lastSpawnTime = Time.time;
    }

    // 释放对象逻辑
    void ReleaseObject()
    {
        if (!_currentObject) return;

        // 启用物理效果
        if (_currentObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.AddForce(_pinchPoint.forward * 5f, ForceMode.Impulse);
        }

        _currentObject = null;
    }

    // 持续更新对象位置
    void HandleObjectPosition()
    {
        if (_currentObject)
        {
            _currentObject.transform.position = _pinchPoint.position;
            _currentObject.transform.rotation = _pinchPoint.rotation;
        }
    }

    // 切换预制体选择（可扩展为手柄按键切换）
    void HandlePrefabSelection()
    {
        // 示例：使用AB键切换预制体
        if (OVRInput.GetDown(OVRInput.Button.One)) // A键
            _currentPrefabIndex = (_currentPrefabIndex + 1) % _prefabs.Length;

        if (OVRInput.GetDown(OVRInput.Button.Two)) // B键
            _currentPrefabIndex = (_currentPrefabIndex - 1 + _prefabs.Length) % _prefabs.Length;
    }
    public void EnableSpawn()
    {
        handInTheTree = true;
    }
    public void DisableSpawn()
    {
        handInTheTree = false;
    }

    // 编辑器可视化
    void OnDrawGizmos()
    {
        if (_pinchPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_pinchPoint.position, 0.02f);
            Gizmos.DrawLine(_pinchPoint.position, _pinchPoint.position + _pinchPoint.forward * 0.1f);
        }
    }
}