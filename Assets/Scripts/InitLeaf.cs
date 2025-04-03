using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Meta.XR.MultiplayerBlocks.Shared;
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
    public  NetworkRunner runner;
    public OVRHand righthand;
    

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
        
        leafInFinger =  Instantiate(leaf,fingerPos.position,fingerPos.rotation,fingerPos);
        //leafInFinger = leafPool.GetObject().gameObject;
        
        //leafInFinger.transform.position = fingerPos.position;
        //leafInFinger.transform.rotation = fingerPos.rotation;
        //Debug.LogWarning("prefab Name:"+leaf.name);
        //Debug.LogWarning("finger Pos:"+fingerPos.position+" "+ fingerPos.rotation);
        //Debug.LogWarning("If with NetworkObject:" +leaf.GetComponent<NetworkObject>().enabled);
        //leafInFinger = runner.Spawn(leaf, fingerPos.position, fingerPos.rotation);
        SwitchLeafState(true);
        Debug.Log("生成树叶");
    }
    public void OnPinchReleased()
    {
        if (leafInFinger == null)
            return;

        Debug.Log("释放树叶");
        SwitchLeafState(false);
        Invoke("DestroyLeaf", 3f);
    }

    void SwitchLeafState(bool inFinger)
    {
        if (inFinger)
        {
            leafInFinger.transform.SetParent(fingerPos);
            leafInFinger.GetComponent<Rigidbody>().freezeRotation = true;   
            leafInFinger.GetComponent<Rigidbody>().useGravity = false;
            //leafInFinger.GetComponent<TransferOwnershipOnSelect>().UseGravity = false;
            leafInFinger.GetComponent<LeafFollowFinger>().enabled = true;
        }
        else
        {
            leafInFinger.transform.SetParent(null);
            leafInFinger.GetComponent<Rigidbody>().freezeRotation = false;
            leafInFinger.GetComponent<Rigidbody>().useGravity = true;
            //leafInFinger.GetComponent<TransferOwnershipOnSelect>().UseGravity = true;
            leafInFinger.GetComponent<LeafFollowFinger>().enabled = false;
        }
    }

    void DestroyLeaf()
    {
        Destroy(leafInFinger);
        Debug.Log("树叶销毁");
    }
}
