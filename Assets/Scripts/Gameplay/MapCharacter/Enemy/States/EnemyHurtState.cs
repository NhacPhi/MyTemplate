using UnityEngine;

namespace Gameplay.MapCharacter.Enemy.States
{
    public class EnemyHurtState : EnemyState
    {
        private float hurtTimer;
        private float hurtDuration = 0.5f; // Thời gian bị choáng/giật lùi

        public EnemyHurtState(EnemyAIController controller) : base(controller) { }

        public override void Enter()
        {
            hurtTimer = 0f;
            if (controller.Anim != null)
            {
                controller.Anim.SetBool("IsMove", false);
                controller.Anim.SetTrigger("Hurt");
            }
            
            Debug.Log($"Enemy Hurt! HP remaining: {controller.CurrentHP}");
        }

        public override void Update()
        {
            hurtTimer += Time.deltaTime;
            if (hurtTimer >= hurtDuration)
            {
                // Sau khi hết choáng, quay lại đánh nếu có mục tiêu, không thì về Idle
                if (controller.Target != null)
                {
                    if (controller.AIType == EnemyAIType.Coward)
                    {
                        controller.ChangeState(controller.FleeState);
                    }
                    else
                    {
                        controller.ChangeState(controller.AttackState);
                    }
                }
                else
                {
                    controller.ChangeState(controller.IdleState);
                }
            }
        }
    }
}
