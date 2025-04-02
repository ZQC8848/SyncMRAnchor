using System.Collections;
using System.Collections.Generic;
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
