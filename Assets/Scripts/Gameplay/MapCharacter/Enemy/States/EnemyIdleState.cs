using UnityEngine;

namespace Gameplay.MapCharacter.Enemy.States
{
    public class EnemyIdleState : EnemyState
    {
        private float idleTimer;
        private float idleDuration = 2f; // Thời gian đứng yên trước khi chuyển sang Walk

        public EnemyIdleState(EnemyAIController controller) : base(controller) { }

        public override void Enter()
        {
            idleTimer = 0f;
            if (controller.Anim != null)
            {
                controller.Anim.SetBool("IsMove", false);
            }
        }

        public override void Update()
        {
            // Logic đếm thời gian chuyển sang trạng thái Walk (Tuần tra)
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
            {
                controller.ChangeState(controller.WalkState);
            }
        }
    }
}
