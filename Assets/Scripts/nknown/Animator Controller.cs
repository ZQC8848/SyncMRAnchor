using UnityEngine;

public class RandomAnimTrigger : MonoBehaviour
{
    private Animator animator;
    public int animationCount = 3;  // 动画状态总数（根据实际数量修改）
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 示例：按下空格键触发随机动画
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int randomState;

             randomState = (int)Random.Range(1, animationCount);

            if (randomState == 1 && Random.Range(-1f, 1f) >0)
            {
                RunSwitchWalk();
            }
            else if(randomState == 2 && Random.Range(-1f, 1f) > 0)
            {
                WalkSwitchRun();
            }
            // 更新参数并记录当前状态
            animator.SetInteger("RandomState", randomState);
        }
    }
    public void ClearRamdomValue()
    {
        animator.SetInteger("RandomState", 0);
    }

    public void WalkSwitchRun()
    {
        animator.SetTrigger("walkSwitchRun");
    }

    public void RunSwitchWalk()
    {
        animator.SetTrigger("runSwitchWalk");
    }
}