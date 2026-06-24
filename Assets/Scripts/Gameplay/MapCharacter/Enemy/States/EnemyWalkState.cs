using UnityEngine;

namespace Gameplay.MapCharacter.Enemy.States
{
    public class EnemyWalkState : EnemyState
    {
        private Vector3 targetPosition;
        private float walkTimer;
        private float walkDuration = 3f;

        public EnemyWalkState(EnemyAIController controller) : base(controller) { }

        public override void Enter()
        {
            walkTimer = 0f;

            // Chọn một điểm ngẫu nhiên xung quanh để đi tới trên mặt phẳng XZ
            Vector2 randomCircle = Random.insideUnitCircle * 3f;
            Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
            targetPosition = controller.transform.position + randomDirection;

            if (controller.Anim != null)
            {
                controller.Anim.SetBool("IsMove", true);
                Vector3 dir = (targetPosition - controller.transform.position).normalized;
                controller.Anim.SetFloat("MoveX", Mathf.Round(dir.x));
                controller.Anim.SetFloat("MoveY", Mathf.Round(-dir.z)); // Đảo ngược hướng Z để khớp với Animator Blend Tree
            }
        }

        public override void Update()
        {
            controller.MoveTowards(targetPosition);

            // Kiểm tra đến nơi hoặc hết thời gian Walk
            if (Vector3.Distance(controller.transform.position, targetPosition) < 0.1f)
            {
                controller.ChangeState(controller.IdleState);
                return;
            }

            walkTimer += Time.deltaTime;
            if (walkTimer >= walkDuration)
            {
                controller.ChangeState(controller.IdleState);
            }
        }
    }
}
