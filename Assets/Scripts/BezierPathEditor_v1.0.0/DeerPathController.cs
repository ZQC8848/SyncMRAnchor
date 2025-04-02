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
        rb.velocity = Vector3.forward * originalSpeed; // ��ʼ�ƶ�
    }

    void Update()
    {
        // ������⵱ǰλ��
        if (Vector3.Distance(transform.position, targetPos1) < 0.1f)
        {
            StartCoroutine(HandleTarget1Behavior());
        }
        else if (Vector3.Distance(transform.position, targetPos2) < 0.1f)
        {
            StartCoroutine(HandleTarget2Behavior());
        }

        // ��������
        animator.SetFloat("Speed", rb.velocity.magnitude);
    }

    IEnumerator HandleTarget1Behavior()
    {
        rb.velocity = Vector3.zero; // �ٶȹ���
        animator.Play("Idle");      // ���Ŵ�������[2,10](@ref)

        yield return new WaitForSeconds(10f); // �ȴ�10��[6](@ref)

        rb.velocity = Vector3.forward * originalSpeed; // �ָ�ԭ��
    }

    IEnumerator HandleTarget2Behavior()
    {
        float boostDuration = 5f;
        float boostSpeed = 0.5f;

        rb.velocity = Vector3.forward * boostSpeed;
        animator.Play("Run"); // ���ű��ܶ���[2,9](@ref)

        yield return new WaitForSeconds(boostDuration);

        rb.velocity = Vector3.forward * originalSpeed; // �ָ������ٶ�
    }
}