using UnityEngine;
using OVR.Input;

public class OculusHandSpawner : MonoBehaviour
{
    [Header("�ֲ�����")]
    [SerializeField] private OVRHand _rightHand; // ������OVRHand���
    [SerializeField] private Transform _pinchPoint; // OculusHandPinchArrowBlended�ڵ�

    [Header("��������")]
    [SerializeField] private GameObject[] _prefabs; // ��ק���Ԥ���嵽�˴�
    [SerializeField] private float _spawnCooldown = 0.5f; // ������ȴʱ��

    private GameObject _currentObject;     // ��ǰ���ɵĶ���
    private bool _wasPinching;            // ��һ֡���״̬
    private float _lastSpawnTime;         // �ϴ�����ʱ��
    private int _currentPrefabIndex;      // ��ǰѡ���Ԥ��������

    public bool handInTheTree;//�ж����Ƿ���������

    void Start()
    {
        // �Զ����ҹؼ��ڵ�
        if (!_pinchPoint)
            _pinchPoint = GameObject.Find("OculusHandPinchArrowBlended").transform;
    }

    void Update()
    {
        HandlePinchGesture();
        HandleObjectPosition();
        HandlePrefabSelection();
    }

    // ���Ƽ���߼�
    void HandlePinchGesture()
    {
        bool isPinching = _rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // ����״̬�仯ʱ����
        if (isPinching != _wasPinching)
        {
            if (isPinching) TrySpawnPrefab();
            else ReleaseObject();
        }

        _wasPinching = isPinching;
    }

    // ��������Ԥ����
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

        // ��ʼ���������
        if (_currentObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        _lastSpawnTime = Time.time;
    }

    // �ͷŶ����߼�
    void ReleaseObject()
    {
        if (!_currentObject) return;

        // ��������Ч��
        if (_currentObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.AddForce(_pinchPoint.forward * 5f, ForceMode.Impulse);
        }

        _currentObject = null;
    }

    // �������¶���λ��
    void HandleObjectPosition()
    {
        if (_currentObject)
        {
            _currentObject.transform.position = _pinchPoint.position;
            _currentObject.transform.rotation = _pinchPoint.rotation;
        }
    }

    // �л�Ԥ����ѡ�񣨿���չΪ�ֱ������л���
    void HandlePrefabSelection()
    {
        // ʾ����ʹ��AB���л�Ԥ����
        if (OVRInput.GetDown(OVRInput.Button.One)) // A��
            _currentPrefabIndex = (_currentPrefabIndex + 1) % _prefabs.Length;

        if (OVRInput.GetDown(OVRInput.Button.Two)) // B��
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

    // �༭�����ӻ�
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