using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IImpactSkill 
{
    void OnProjectileImpact(Entity target, Vector2 contactPoint);
}
