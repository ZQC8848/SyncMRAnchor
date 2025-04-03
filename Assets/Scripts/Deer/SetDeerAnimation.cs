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
        public float speed; // 当前速度（用于Inspector调试）
        public Animator animator; // 动画控制器
        private Coroutine speedTransitionCoroutine; // 速度过渡协程引用
        private SimpleBezierAlonger bezier; // 缓存的路径跟随组件
        public NetworkMecanimAnimator networkAnimator;
        public ITransferOwnership transferOwnership;

        void Start()
        {
            // 缓存组件提升性能
            bezier = GetComponent<SimpleBezierAlonger>();
            transferOwnership = this.GetInterfaceComponent<ITransferOwnership>();
        }

        void Update()
        {
            // 实时同步速度到动画器
            animator.SetFloat("Speed", bezier.speed);
            speed = bezier.speed; // 仅用于Inspector显示
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
        /// 平滑设置移动速度
        /// </summary>
        /// <param name="targetSpeed">目标速度</param>
        /// <param name="transitionTime">过渡时间（秒）</param>
        public void SetSpeed(float targetSpeed, float transitionTime = 1.0f)
        {
            // 如果已有过渡在进行则停止
            if (speedTransitionCoroutine != null)
            {
                StopCoroutine(speedTransitionCoroutine);
            }

            // 启动新的速度过渡
            speedTransitionCoroutine = StartCoroutine(SmoothSpeedTransition(
                bezier.speed,
                targetSpeed,
                transitionTime
            ));
        }

        /// <summary>
        /// 平滑速度过渡协程
        /// </summary>
        private IEnumerator SmoothSpeedTransition(float startSpeed, float endSpeed, float transitionTime)
        {
            float elapsed = 0;
            float currentSpeed = startSpeed;
            float smoothVelocity = 0; // 正确的速率跟踪变量

            // 计算合理的平滑时间（建议为总时间的1/3）
            float smoothTime = Mathf.Max(transitionTime * 0.3f, 0.01f);

            while (elapsed < transitionTime)
            {
                // 计算插值比例（0-1）
                float t = Mathf.Clamp01(elapsed / transitionTime);

                // 使用SmoothDamp正确实现
                currentSpeed = Mathf.SmoothDamp(
                    currentSpeed,
                    endSpeed,
                    ref smoothVelocity,
                    smoothTime
                );

                // 更新实际速度
                bezier.speed = currentSpeed;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 强制确保最终值准确
            bezier.speed = endSpeed;

            // 处理浮点精度残留
            if (Mathf.Abs(bezier.speed) < 0.001f)
            {
                bezier.speed = 0;
            }
        }
    }
}