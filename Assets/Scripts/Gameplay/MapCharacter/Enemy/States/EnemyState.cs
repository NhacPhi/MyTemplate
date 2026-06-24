using UnityEngine;
using Gameplay.Common;

namespace Gameplay.MapCharacter.Enemy
{
    public abstract class EnemyState
    {
        protected EnemyAIController controller;

        public EnemyState(EnemyAIController controller)
        {
            this.controller = controller;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }
}
