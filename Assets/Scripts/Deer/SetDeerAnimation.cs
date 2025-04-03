using System.Collections;
using System.Collections.Generic;
using Fusion;
using Meta.XR.BuildingBlocks;
using Meta.XR.MultiplayerBlocks.Shared;
using UnityEngine;
using Meta.XR.MultiplayerBlocks.Fusion;

namespace Meta.XR.MultiplayerBlocks.Fusion
{
    public interface ITransferOwnership
    {
        /// <summary>
        /// Transfers the ownership of the networked game object to the local player.
        /// </summary>
        public void TransferOwnershipToLocalPlayer();

        /// <summary>
        /// Indicates whether the local player has ownership of the networked game object.
        /// </summary>
        /// <returns>'true' if the local player has ownership of the networked game object</returns>
        public bool HasOwnership();
    }

    public class SetDeerAnimation : MonoBehaviour
    {
        public float speed; // ��ǰ�ٶȣ�����Inspector���ԣ�
        public Animator animator; // ����������
        private Coroutine speedTransitionCoroutine; // �ٶȹ���Э������
        private SimpleBezierAlonger bezier; // �����·���������
        public NetworkMecanimAnimator networkAnimator;
        public ITransferOwnership transferOwnership;

        void Start()
        {
            // ���������������
            bezier = GetComponent<SimpleBezierAlonger>();
            transferOwnership = this.GetInterfaceComponent<ITransferOwnership>();
        }

        void Update()
        {
            // ʵʱͬ���ٶȵ�������
            animator.SetFloat("Speed", bezier.speed);
            speed = bezier.speed; // ������Inspector��ʾ
        }

        public void SetDeerAnimTrigger(string animName)
        {
            if (!transferOwnership.HasOwnership())
            {
                transferOwnership.TransferOwnershipToLocalPlayer();
            }

            //if (lastTouchTime <= 0) return;
            //animator.SetTrigger(animName);
            networkAnimator.SetTrigger(animName);
        }

        public void SetDeerAnimBool(string animName, bool value)
        {
            networkAnimator.Animator.SetBool(animName, value);
        }

        public void RandomSetSpeed()
        {
            //float randomResult = Random.Range(0f, 1f);
            //if( randomResult < 0.8)
            //{
            //    SetSpeed(0, 0.5f);
            //}
            //else if(randomResult > 0.8)
            //{
            //    SetSpeed(0.1f, 0.5f);
            //}
            SetSpeed(0, 0.1f);
        }

        /// <summary>
        /// ƽ�������ƶ��ٶ�
        /// </summary>
        /// <param name="targetSpeed">Ŀ���ٶ�</param>
        /// <param name="transitionTime">����ʱ�䣨�룩</param>
        public void SetSpeed(float targetSpeed, float transitionTime = 1.0f)
        {
            // ������й����ڽ�����ֹͣ
            if (speedTransitionCoroutine != null)
            {
                StopCoroutine(speedTransitionCoroutine);
            }

            // �����µ��ٶȹ���
            speedTransitionCoroutine = StartCoroutine(SmoothSpeedTransition(
                bezier.speed,
                targetSpeed,
                transitionTime
            ));
        }

        /// <summary>
        /// ƽ���ٶȹ���Э��
        /// </summary>
        private IEnumerator SmoothSpeedTransition(float startSpeed, float endSpeed, float transitionTime)
        {
            float elapsed = 0;
            float currentSpeed = startSpeed;
            float smoothVelocity = 0; // ��ȷ�����ʸ��ٱ���

            // ��������ƽ��ʱ�䣨����Ϊ��ʱ���1/3��
            float smoothTime = Mathf.Max(transitionTime * 0.3f, 0.01f);

            while (elapsed < transitionTime)
            {
                // �����ֵ������0-1��
                float t = Mathf.Clamp01(elapsed / transitionTime);

                // ʹ��SmoothDamp��ȷʵ��
                currentSpeed = Mathf.SmoothDamp(
                    currentSpeed,
                    endSpeed,
                    ref smoothVelocity,
                    smoothTime
                );

                // ����ʵ���ٶ�
                bezier.speed = currentSpeed;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // ǿ��ȷ������ֵ׼ȷ
            bezier.speed = endSpeed;

            // �����㾫�Ȳ���
            if (Mathf.Abs(bezier.speed) < 0.001f)
            {
                bezier.speed = 0;
            }
        }
    }
}