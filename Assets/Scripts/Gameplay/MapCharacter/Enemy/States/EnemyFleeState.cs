using UnityEngine;
using Gameplay.Common;

namespace Gameplay.MapCharacter.Enemy.States
{
    public class EnemyFleeState : EnemyState
    {
        public EnemyFleeState(EnemyAIController controller) : base(controller) { }

        public override void Enter()
        {
            if (controller.Anim != null)
            {
                controller.Anim.SetBool("IsRun", true);
            }
        }

        public override void Update()
        {
            if (controller.Target == null)
            {
                controller.ChangeState(controller.IdleState);
                return;
            }

            // Tính tọa độ 2D của Player
            Vector3 targetPosXZ = new Vector3(controller.Target.position.x, controller.transform.position.y, controller.Target.position.z);
            
            // Tính hướng chạy trốn (ngược lại với hướng về phía Player)
            Vector3 dir = (controller.transform.position - targetPosXZ).normalized;

            // Tính điểm đến để bỏ chạy
            Vector3 fleeDestination = controller.transform.position + dir * 2f;
            
            // Di chuyển với tốc độ RunSpeed
            controller.MoveTowards(fleeDestination, controller.RunSpeed);

            // Cập nhật Animation
            if (controller.Anim != null)
            {
                controller.Anim.SetFloat("MoveX", Mathf.Round(dir.x));
                controller.Anim.SetFloat("MoveY", Mathf.Round(-dir.z));
            }
        }

        public override void Exit()
        {
            if (controller.Anim != null)
            {
                controller.Anim.SetBool("IsRun", false);
            }
        }
    }
}
