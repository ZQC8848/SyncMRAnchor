using UnityEngine;

/// <summary>
/// 随机待机动画
/// </summary>
//状态数量

public class idledeer : StateMachineBehaviour
{
    public int stateCount;
    //最短等待时间
    public float minWaitTime = 5f;
    //最长等待时间
    public float maxWaitTime = 15f;
    //两个动画之间的时间间隔
    private float m_Interval;
    //待机状态属性
    private readonly int m_HashRandomIdle = Animator.StringToHash("Idle");
    //下一个状态
    private int m_NextState;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
     override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
     {
        //进入默认待机动画时，计算时间间隔和下一状态
        m_Interval = Random.Range(minWaitTime, maxWaitTime);
        m_NextState = Random.Range(0,stateCount);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(stateInfo.normalizedTime > m_Interval && !animator.IsInTransition(0)) 
        {
            animator.SetInteger(m_HashRandomIdle, m_NextState);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //t退出时默认设为-1
        animator.SetInteger(m_HashRandomIdle, -1);
    }
}
