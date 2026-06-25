using UnityEngine;
using System.Collections.Generic;
using Gameplay.Common;
using Gameplay.MapCharacter.Enemy.States;

namespace Gameplay.MapCharacter.Enemy
{
    public enum EnemyAIType
    {
        Aggressive, // Quái vật: Tấn công
        Coward      // Động vật: Bỏ chạy
    }

    public class EnemyAIController : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        public int MaxHP = 100;
        public int CurrentHP { get; private set; }
        public int AttackDamage = 10;
        public float MoveSpeed = 2f;
        public float RunSpeed = 4f; // Tốc độ bỏ chạy
        public float AttackRange = 1.5f;
        public float AttackCooldown = 2f; // Thời gian đếm ngược giữa các đòn đánh
        
        [Header("AI Type")]
        public EnemyAIType AIType = EnemyAIType.Aggressive;

        [Header("Movement Logic")]
        public LayerMask groundLayer;


        [Header("References")]
        public Transform Target;
        public EnemySensor Sensor;
        public Animator Anim;
        
        // State Machine
        private EnemyState currentState;
        public EnemyIdleState IdleState;
        public EnemyWalkState WalkState;
        public EnemyAttackState AttackState;
        public EnemyFleeState FleeState;
        public EnemyHurtState HurtState;
        public EnemyDieState DieState;

        private void Awake()
        {
            CurrentHP = MaxHP;
            
            // Khởi tạo các States
            IdleState = new EnemyIdleState(this);
            WalkState = new EnemyWalkState(this);
            AttackState = new EnemyAttackState(this);
            FleeState = new EnemyFleeState(this);
            HurtState = new EnemyHurtState(this);
            DieState = new EnemyDieState(this);

            if (Anim == null) Anim = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            ChangeState(IdleState);
            
            if (Sensor != null)
            {
                Sensor.OnPlayerEnter += HandlePlayerEnter;
                Sensor.OnPlayerExit += HandlePlayerExit;
            }
        }

        private void Update()
        {
            if (currentState != null)
                currentState.Update();
        }

        public void ChangeState(EnemyState newState)
        {
            if (currentState == DieState) return; // Nếu đã chết thì không đổi state khác
            if (currentState == newState) return; // Tránh việc gọi lại state hiện tại liên tục
            
            if (currentState != null)
                currentState.Exit();

            currentState = newState;
            currentState.Enter();
        }

        private void HandlePlayerEnter(Transform playerTransform)
        {
            if (currentState == DieState) return;
            Target = playerTransform;
            
            if (AIType == EnemyAIType.Aggressive)
            {
                ChangeState(AttackState); // Phát hiện người chơi -> chuyển sang trạng thái tấn công/đuổi theo
            }
            else if (AIType == EnemyAIType.Coward)
            {
                ChangeState(FleeState); // Quái nhút nhát -> bỏ chạy
            }
        }

        private void HandlePlayerExit(Transform playerTransform)
        {
            if (currentState == DieState) return;
            if (Target == playerTransform)
            {
                Target = null;
                ChangeState(IdleState); // Người chơi rời khỏi vùng -> quay lại Idle/Walk
            }
        }

        public void TakeDamage(int damage)
        {
            if (currentState == DieState) return;

            CurrentHP -= damage;
            
            if (CurrentHP <= 0)
            {
                CurrentHP = 0;
                ChangeState(DieState);
            }
            else
            {
                ChangeState(HurtState);
            }
        }
        
        // Helper function di chuyển có check chạm đất (giống Protagonist)
        public void MoveTowards(Vector3 destination, float customSpeed = -1f)
        {
            float speedToUse = customSpeed > 0f ? customSpeed : MoveSpeed;
            Vector3 nextPosition = Vector3.MoveTowards(transform.position, destination, speedToUse * Time.deltaTime);

            // Dùng SphereCast thay cho Raycast để Enemy cũng có thể lướt qua khe nứt
            Vector3 origin = nextPosition - Vector3.forward + Vector3.up * 0.5f;
            if (Physics.SphereCast(origin, 0.4f, Vector3.down, out RaycastHit hit, 5f, groundLayer))
            {
                transform.position = nextPosition;
            }
        }

        private void OnDestroy()
        {
            if (Sensor != null)
            {
                Sensor.OnPlayerEnter -= HandlePlayerEnter;
                Sensor.OnPlayerExit -= HandlePlayerExit;
            }
        }
    }
}
