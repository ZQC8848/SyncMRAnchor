using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class ControlDeerSpeedBox : MonoBehaviour
{
    [Header("触碰到方块后切换速度")]
    public float touchSpeed;
    [Header("过渡时间")]
    public float changeTouchTime;
    [Header("持续时间")]
    public float continueTime;

    [Header("持续时间过后移动速度")]
    public float afterSpeed;
    [Header("过渡时间")]
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
            Debug.Log("开始鹿交互");
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
        Debug.Log("禁止鹿交互");
    }

}
