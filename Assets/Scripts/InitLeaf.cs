using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OVR.Input;
using Oculus.Interaction;
public class InitLeaf : MonoBehaviour
{
    public bool handInTheTree;//�ж����Ƿ���������

    private bool isPinching;
    public GameObject leaf;
    public Transform fingerPos;
    GameObject leafInFinger;

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
        Debug.Log("������Ҷ");
    }
    public void OnPinchReleased()
    {
        if (leafInFinger == null)
            return;

        Debug.Log("�ͷ���Ҷ");
        leafInFinger.GetComponent<LeafFollowFinger>().enabled = false;
        leafInFinger.GetComponent<Rigidbody>().useGravity = true;
        leafInFinger.transform.SetParent(null);
    }
}
