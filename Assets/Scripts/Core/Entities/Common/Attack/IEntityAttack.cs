using Cysharp.Threading.Tasks;
using System;

public enum AttackType 
{
    Melee,
    Range,
    Aoe
}
public interface IEntityAttack
{
    UniTask ExecuteAttack(EntityStateData data);
}
