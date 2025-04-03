using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using Meta.XR.MultiplayerBlocks.Fusion;
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
    [Header("交互提示")]
    public GameObject infoUI;
    [Header("鹿动画控制器")]
    public SetDeerAnimation deerAnimationController;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Deer"))
        {
            deerAnimationController.SetSpeed(touchSpeed, changeTouchTime);
            
            StartCoroutine(ContinueMove(deerAnimationController));
            //unityEventWrapper.enabled = true;
            deerAnimationController.SetDeerAnimBool("IsMoving", false);
            Debug.Log("开始鹿交互");
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
