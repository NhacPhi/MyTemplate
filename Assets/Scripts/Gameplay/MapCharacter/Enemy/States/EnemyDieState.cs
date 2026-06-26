using UnityEngine;

namespace Gameplay.MapCharacter.Enemy.States
{
    public class EnemyDieState : EnemyState
    {
        private bool isDead = false;
        private float failSafeTimer = 0f;

        public EnemyDieState(EnemyAIController controller) : base(controller) { }

        public override void Enter()
        {
            isDead = false;
            failSafeTimer = 0f;

            if (controller.Anim != null)
            {
                controller.Anim.SetBool("IsMove", false);
                controller.Anim.SetTrigger("Die");
            }

            // Vô hiệu hóa Collider để không bị đánh tiếp hoặc cản đường
            Collider col = controller.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Vô hiệu hóa vùng phát hiện
            if (controller.Sensor != null) controller.Sensor.gameObject.SetActive(false);
        }

        public override void Update()
        {
            if (isDead) return;

            if (controller.Anim != null)
            {
                AnimatorStateInfo stateInfo = controller.Anim.GetCurrentAnimatorStateInfo(0);
                
                // Kiểm tra xem Animator đã chuyển sang State có tên "DeathTree" chưa
                if (stateInfo.IsName("DeathTree"))
                {
                    // normalizedTime >= 1.0f nghĩa là vòng đời animation đã chạy xong 100%
                    if (stateInfo.normalizedTime >= 1.0f)
                    {
                        isDead = true;
                        controller.StartBlinkAndDeactivate();
                    }
                }
            }

            // Đề phòng trường hợp Animator bị lag không nhận Trigger, sau 3 giây cứ ép nó biến mất
            failSafeTimer += Time.deltaTime;
            if (failSafeTimer >= 3f)
            {
                isDead = true;
                controller.StartBlinkAndDeactivate();
            }
        }
    }
}
