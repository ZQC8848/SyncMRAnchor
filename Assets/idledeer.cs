using UnityEngine;

/// <summary>
/// �����������
/// </summary>
//״̬����

public class idledeer : StateMachineBehaviour
{
    public int stateCount;
    //��̵ȴ�ʱ��
    public float minWaitTime = 5f;
    //��ȴ�ʱ��
    public float maxWaitTime = 15f;
    //��������֮���ʱ����
    private float m_Interval;
    //����״̬����
    private readonly int m_HashRandomIdle = Animator.StringToHash("Idle");
    //��һ��״̬
    private int m_NextState;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
     override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
     {
        //����Ĭ�ϴ�������ʱ������ʱ��������һ״̬
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
        //t�˳�ʱĬ����Ϊ-1
        animator.SetInteger(m_HashRandomIdle, -1);
    }
}
