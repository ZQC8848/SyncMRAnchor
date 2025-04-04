using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using Meta.XR.MultiplayerBlocks.Fusion;
using Random = UnityEngine.Random;

public class ControlDeerSpeedBox : MonoBehaviour
{
    [Header("������������л��ٶ�")]
    public float touchSpeed;
    [Header("����ʱ��")]
    public float changeTouchTime;
    [Header("����ʱ��")]
    public float continueTime;

    [Header("����ʱ������ƶ��ٶ�")]
    public float afterSpeed_1=0.1f;
    public float afterSpeed_2=0.3f;
    [Header("����ʱ��")]
    public float changeAfterTime;
    [Header("������ʾ")]
    public GameObject infoUI;
    [Header("¹����������")]
    public SetDeerAnimation deerAnimationController;
    
    private SwitchTutorialState switchTutorialState;

    private void Start()
    {
        switchTutorialState = FindObjectOfType<SwitchTutorialState>();
    }

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
        if (Random.Range(-1.0f, 1.0f) < 0f)
        {
            setDeerAnimation.SetSpeed(afterSpeed_1, changeAfterTime);
        }
        else
        {
            setDeerAnimation.SetSpeed(afterSpeed_2, changeAfterTime);
        }
        infoUI.SetActive(false);
    }



}
