using UnityEngine;
using Gameplay.Common;

namespace Gameplay.MapCharacter.Enemy.States
{
    public class EnemyAttackState : EnemyState
    {
        private float attackCooldownTimer;

        public EnemyAttackState(EnemyAIController controller) : base(controller) { }

        public override void Enter()
        {
            // Cho phép đánh ngay đòn đầu tiên khi vừa tiếp cận, sau đó mới đếm ngược
            attackCooldownTimer = controller.AttackCooldown; 
            if (controller.Anim != null)
            {
                controller.Anim.SetBool("IsMove", true);
            }
        }

        public override void Update()
        {
            if (controller.Target == null)
            {
                controller.ChangeState(controller.IdleState);
                return;
            }

            // Đếm ngược thời gian hồi chiêu liên tục
            attackCooldownTimer += Time.deltaTime;

            // Tính khoảng cách 2D (trên mặt phẳng XZ) để bỏ qua sai lệch về chiều cao (Y)
            Vector3 targetPosXZ = new Vector3(controller.Target.position.x, controller.transform.position.y, controller.Target.position.z);
            float distanceToTarget = Vector3.Distance(controller.transform.position, targetPosXZ);

            if (distanceToTarget > controller.AttackRange)
            {
                // Chưa đủ tầm -> di chuyển tới Target (nhưng khóa trục Y để không bị lún/bay lên)
                controller.MoveTowards(targetPosXZ);
                
                if (controller.Anim != null)
                {
                    controller.Anim.SetBool("IsMove", true);
                    Vector3 dir = (targetPosXZ - controller.transform.position).normalized;
                    controller.Anim.SetFloat("MoveX", Mathf.Round(dir.x));
                    controller.Anim.SetFloat("MoveY", Mathf.Round(-dir.z));
                }
            }
            else
            {
                // Đủ tầm -> Tấn công
                if (controller.Anim != null) controller.Anim.SetBool("IsMove", false);

                if (attackCooldownTimer >= controller.AttackCooldown)
                {
                    PerformAttack();
                    attackCooldownTimer = 0f; // Reset đếm ngược sau khi đánh
                }
            }
        }

        private void PerformAttack()
        {
            if (controller.Anim != null)
            {
                // Ghi chú: User không đề cập Trigger Attack, nhưng cần để vung tay/đánh. 
                // Tôi sẽ gọi Trigger "Attack". Nếu bạn dùng Bool thì đổi lại nhé.
                controller.Anim.SetTrigger("Attack");
                
                // Cập nhật hướng xoay mặt về Player khi đánh (chỉ tính trên XZ)
                Vector3 targetPosXZ = new Vector3(controller.Target.position.x, controller.transform.position.y, controller.Target.position.z);
                Vector3 dir = (targetPosXZ - controller.transform.position).normalized;
                controller.Anim.SetFloat("MoveX", Mathf.Round(dir.x));
                controller.Anim.SetFloat("MoveY", Mathf.Round(-dir.z));
            }

            // Gây sát thương lên Target
            IDamageable damageable = controller.Target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(controller.AttackDamage);
                Debug.Log($"Enemy attacked Player for {controller.AttackDamage} damage!");
            }
        }
    }
}
