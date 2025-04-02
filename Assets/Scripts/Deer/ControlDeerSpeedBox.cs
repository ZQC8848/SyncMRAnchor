using System.Collections;
using System.Collections.Generic;
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
        }
    }

    IEnumerator ContinueMove(SetDeerAnimation setDeerAnimation)
    {
        yield return new WaitForSeconds(continueTime);

        setDeerAnimation.SetSpeed(afterSpeed, changeAfterTime);
    }

}
