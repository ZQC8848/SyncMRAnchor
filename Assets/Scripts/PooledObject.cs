using UnityEngine;
/// <summary>
/// 所有需要使用对象池机制的对象的基类
/// </summary>
public class PooledObject : MonoBehaviour
{
    // 归属的池
    public ObjectPool Pool { get; set; }

    // 场景中某个具体的池（不可序列化）
    [System.NonSerialized]
    private ObjectPool poolInstanceForPrefab;

    /// <summary>
    /// 回收对象到对象池中
    /// </summary>
    public void ReturnToPool()
    {
        if (Pool)
        {
            Pool.AddObject(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 返回对象池中可用对象的实例
    /// </summary>
    public T GetPooledInstance<T>() where T : PooledObject
    {
        if (!poolInstanceForPrefab)
        {
            poolInstanceForPrefab = ObjectPool.GetPool(this);
        }
        return (T)poolInstanceForPrefab.GetObject();
    }
}