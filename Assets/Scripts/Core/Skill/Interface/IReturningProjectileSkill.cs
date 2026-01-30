using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;


public interface IReturningProjectileSkill 
{
    // Projectile
    GameObject ActiveProjectiles { get; }

    // Handle animation (Throw or delay before the object appears)
    UniTask ThrowProjectile(SkillData data, Entity caster);

    // Callback Handling logic when a collision is detected
    void OnProjectileHit(Entity target, GameObject projectile);

    // Callback return object 
    void OnProjectileReturned(GameObject projectitle);
}
