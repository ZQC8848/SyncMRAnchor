using UnityEngine;

public class RandomAnimationPlayer : MonoBehaviour
{
    private Animator animator;
    public string[] triggerNames; // 在Inspector面板设置触发器名称

    void Start()
    {
        animator = GetComponent<Animator>();
        PlayRandomAnimation();
    }

    public void PlayRandomAnimation()
    {
        // 重置所有触发器
        foreach (var trigger in triggerNames)
        {
            animator.ResetTrigger(trigger);
        }

        // 随机选择触发器
        int randomIndex = Random.Range(0, triggerNames.Length);
        animator.SetTrigger(triggerNames[randomIndex]);
    }
}