using UnityEngine;

namespace Gameplay.Common
{
    public interface IDamageable
    {
        bool IsTargetable { get; }
        void TakeDamage(int damage);
    }
}
