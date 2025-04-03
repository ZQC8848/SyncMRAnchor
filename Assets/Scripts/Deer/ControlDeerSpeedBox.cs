using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using Meta.XR.MultiplayerBlocks.Fusion;
public class ControlDeerSpeedBox : MonoBehaviour
{
    [Header("������������л��ٶ�")]
    public float touchSpeed;
    [Header("����ʱ��")]
    public float changeTouchTime;
    [Header("����ʱ��")]
    public float continueTime;

    [Header("����ʱ������ƶ��ٶ�")]
    public float afterSpeed;
    [Header("����ʱ��")]
    public float changeAfterTime;
    [Header("������ʾ")]
    public GameObject infoUI;
    [Header("¹����������")]
    public SetDeerAnimation deerAnimationController;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Deer"))
        {
            deerAnimationController.SetSpeed(touchSpeed, changeTouchTime);
            
            StartCoroutine(ContinueMove(deerAnimationController));
            //unityEventWrapper.enabled = true;
            deerAnimationController.SetDeerAnimBool("IsMoving", false);
            Debug.Log("��ʼ¹����");
            infoUI.SetActive(true);
        }
    }

    IEnumerator ContinueMove(SetDeerAnimation setDeerAnimation)
    {
        yield return new WaitForSeconds(continueTime);
        setDeerAnimation.SetDeerAnimBool("IsMoving", true);
        setDeerAnimation.SetSpeed(afterSpeed, changeAfterTime);
        infoUI.SetActive(false);
    }



}
