using UnityEngine;
using System.Collections;

public class DeerPathController : MonoBehaviour
{
    public Vector3 targetPos1 = new Vector3(-4, 0, 0);
    public Vector3 targetPos2 = new Vector3(4, 0, 7);
    private float originalSpeed = 0.1f;
    private Animator animator;
    private Rigidbody rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.forward * originalSpeed; // 初始移动
    }

    void Update()
    {
        // 持续检测当前位置
        if (Vector3.Distance(transform.position, targetPos1) < 0.1f)
        {
            StartCoroutine(HandleTarget1Behavior());
        }
        else if (Vector3.Distance(transform.position, targetPos2) < 0.1f)
        {
            StartCoroutine(HandleTarget2Behavior());
        }

        // 动画控制
        animator.SetFloat("Speed", rb.velocity.magnitude);
    }

    IEnumerator HandleTarget1Behavior()
    {
        rb.velocity = Vector3.zero; // 速度归零
        animator.Play("Idle");      // 播放待机动画[2,10](@ref)

        yield return new WaitForSeconds(10f); // 等待10秒[6](@ref)

        rb.velocity = Vector3.forward * originalSpeed; // 恢复原速
    }

    IEnumerator HandleTarget2Behavior()
    {
        float boostDuration = 5f;
        float boostSpeed = 0.5f;

        rb.velocity = Vector3.forward * boostSpeed;
        animator.Play("Run"); // 播放奔跑动画[2,9](@ref)

        yield return new WaitForSeconds(boostDuration);

        rb.velocity = Vector3.forward * originalSpeed; // 恢复基础速度
    }
}