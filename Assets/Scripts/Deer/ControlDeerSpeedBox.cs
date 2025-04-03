using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

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

    private SetDeerAnimation deerAnimationController;
    
    public InteractableUnityEventWrapper unityEventWrapper;
    private void Start()
    {
        deerAnimationController = GameObject.FindObjectOfType<SetDeerAnimation>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Deer"))
        {
            deerAnimationController.SetSpeed(touchSpeed, changeTouchTime);
            
            StartCoroutine(ContinueMove(deerAnimationController));
            StartCoroutine(DisableTouch());
            unityEventWrapper.enabled = true;
            deerAnimationController.animator.SetBool("IsMoving", false);
            Debug.Log("��ʼ¹����");
        }
    }

    IEnumerator ContinueMove(SetDeerAnimation setDeerAnimation)
    {
        yield return new WaitForSeconds(continueTime);
        setDeerAnimation.animator.SetBool("IsMoving", true);
        setDeerAnimation.SetSpeed(afterSpeed, changeAfterTime);
    }

    IEnumerator DisableTouch()
    {
        yield return new WaitForSeconds(continueTime/2);
        unityEventWrapper.enabled = false;
        Debug.Log("��ֹ¹����");
    }

}
