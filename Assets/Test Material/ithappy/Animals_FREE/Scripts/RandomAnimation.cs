using UnityEngine;

public class RandomAnimationPlayer : MonoBehaviour
{
    private Animator animator;
    public string[] triggerNames; // ��Inspector������ô���������

    void Start()
    {
        animator = GetComponent<Animator>();
        PlayRandomAnimation();
    }

    public void PlayRandomAnimation()
    {
        // �������д�����
        foreach (var trigger in triggerNames)
        {
            animator.ResetTrigger(trigger);
        }

        // ���ѡ�񴥷���
        int randomIndex = Random.Range(0, triggerNames.Length);
        animator.SetTrigger(triggerNames[randomIndex]);
    }
}