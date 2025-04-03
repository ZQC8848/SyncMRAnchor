using System;
using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    // 池中对象prefab
    public PooledObject prefab;

    // 存储可用对象的缓冲区
    private List<PooledObject> availableObjects = new List<PooledObject>();


    private void Start()
    {
        foreach (Transform child in transform)
        {
            PooledObject pooledObject = child.GetComponent<PooledObject>();
            pooledObject.Pool = this;
            pooledObject.ReturnToPool();
        }
        Debug.Log("对象池创建完成，个数："+availableObjects.Count);
    }

    /// <summary>
    /// 从池中取出对象，返回该对象
    /// </summary>
    public PooledObject GetObject()
    {
        PooledObject obj;
        int lastAvailableIndex = availableObjects.Count - 1;
        if (lastAvailableIndex >= 0)
        {
            obj = availableObjects[lastAvailableIndex];
            availableObjects.RemoveAt(lastAvailableIndex);
            obj.gameObject.SetActive(true);
        }
        else // 池中无可用obj
        {
            Debug.LogError("对象池中无可用对象");
            obj = Instantiate<PooledObject>(prefab);
            obj.transform.SetParent(transform, false);
            obj.Pool = this;
        }
        Debug.Log("对象池剩余个数："+availableObjects.Count);
        return obj;
    }

    /// <summary>
    /// 向池中放入obj
    /// </summary>
    public void AddObject(PooledObject obj)
    {
        obj.gameObject.SetActive(false);
        availableObjects.Add(obj);
    }

    /// <summary>
    /// 【静态方法】创建并返回对象所属的对象池
    /// </summary>
    public static ObjectPool GetPool(PooledObject prefab)
    {
        GameObject obj;
        ObjectPool pool;
        // 编辑器模式下检查是否有同名pool存在，防止重复创建pool
        if (Application.isEditor)
        {
            obj = GameObject.Find(prefab.name + " Pool");
            if (obj)
            {
                pool = obj.GetComponent<ObjectPool>();
                if (pool)
                {
                    return pool;
                }
            }
        }
        obj = new GameObject(prefab.name + " Pool");
        DontDestroyOnLoad(obj);
        pool = obj.AddComponent<ObjectPool>();
        pool.prefab = prefab;
        return pool;
    }
}