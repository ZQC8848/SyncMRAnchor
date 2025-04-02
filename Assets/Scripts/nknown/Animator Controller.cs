using UnityEngine;

public class RandomAnimTrigger : MonoBehaviour
{
    private Animator animator;
    public int animationCount = 3;  // ����״̬����������ʵ�������޸ģ�
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // ʾ�������¿ո�������������
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
            // ���²�������¼��ǰ״̬
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