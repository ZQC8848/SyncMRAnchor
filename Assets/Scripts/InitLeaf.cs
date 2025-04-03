using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR.Input;
using Oculus.Interaction;
public class InitLeaf : MonoBehaviour
{
    public bool handInTheTree;//判断手是否在树里面

    private bool isPinching;
    public GameObject leaf;
    public Transform fingerPos;
    GameObject leafInFinger;
    private ObjectPool leafPool;

    public OVRHand righthand;

    private void Start()
    {
        leafPool = FindObjectOfType<ObjectPool>();
    }

    public void EnableSpawn()
    {
        handInTheTree = true;
    }
    public void DisableSpawn()
    {
        handInTheTree = false;
    }
    private void Update()
    {
        if (righthand.GetFingerIsPinching(OVRHand.HandFinger.Index)&&!isPinching)
        {
            isPinching = true;
            OnPinched();
        }
        else if(!righthand.GetFingerIsPinching(OVRHand.HandFinger.Index) && isPinching)
        {
            isPinching = false;
            OnPinchReleased();
        }
    }
    public void OnPinched()
    {
        if (!handInTheTree)
        {
            return;
        }
        //leafInFinger =  Instantiate(leaf,fingerPos.position,fingerPos.rotation,fingerPos);
        leafInFinger = leafPool.GetObject().gameObject;
        leafInFinger.transform.position = fingerPos.position;
        leafInFinger.transform.rotation = fingerPos.rotation;
        leafInFinger.transform.SetParent(fingerPos);
        leafInFinger.GetComponent<Rigidbody>().freezeRotation = true;   
        Debug.Log("生成树叶");
    }
    public void OnPinchReleased()
    {
        if (leafInFinger == null)
            return;

        Debug.Log("释放树叶");
        leafInFinger.GetComponent<LeafFollowFinger>().enabled = false;
        leafInFinger.GetComponent<Rigidbody>().useGravity = true;
        leafInFinger.GetComponent<Rigidbody>().freezeRotation = false;
        leafInFinger.transform.SetParent(leafPool.transform);
        Invoke("ReturnToPool", 3f);
    }

    public void ReturnToPool()
    {
        leafInFinger.GetComponent<PooledObject>().ReturnToPool();
    }
}
