using UnityEngine;

namespace Gameplay.MapCharacter.Enemy.States
{
    public class EnemyDieState : EnemyState
    {
        public EnemyDieState(EnemyAIController controller) : base(controller) { }

        public override void Enter()
        {
            if (controller.Anim != null)
            {
                controller.Anim.SetBool("IsMove", false);
                controller.Anim.SetTrigger("Die");
            }
            
            Debug.Log("Enemy Died!");

            // Vô hiệu hóa Collider để không bị đánh tiếp hoặc cản đường
            Collider col = controller.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Vô hiệu hóa vùng phát hiện
            if (controller.Sensor != null) controller.Sensor.gameObject.SetActive(false);

            // Có thể tự hủy game object sau vài giây (nếu muốn)
            // GameObject.Destroy(controller.gameObject, 2f);
        }

        public override void Update()
        {
            // Không làm gì cả khi đã chết
        }
    }
}
