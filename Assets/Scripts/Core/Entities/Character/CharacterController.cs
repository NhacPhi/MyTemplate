using Cysharp.Threading.Tasks;
using MathNet.Numerics.Statistics.Mcmc;
using System.Collections;
using System.Collections.Generic;
using Tech.Composite;
using UnityEngine;

public class CharacterController : Entity
{
    public override async UniTask ExecuteSkillAsync(SkillCharacter type)
    {
        switch(type)
        {
            case SkillCharacter.Base:
                Debug.Log("Base Attack");
                //var enemy = Target.gameObject.GetComponent<Entity>();
                await gameObject.GetComponent<EntitySkill>().ExecuteSkillAsync(SkillCharacter.Base);
                //HandleTurn(enemy, false);
                break;
            case SkillCharacter.Major:
                Debug.Log("Major SkillCharacter");
                //var enemy = Target.gameObject.GetComponent<Entity>();
                await gameObject.GetComponent<EntitySkill>().ExecuteSkillAsync(SkillCharacter.Major);
                break;

            case SkillCharacter.Ultimate:
                Debug.Log("Ultimate SkillCharacter");
                await gameObject.GetComponent<EntitySkill>().ExecuteSkillAsync(SkillCharacter.Ultimate);
                break;
        }
    }


    protected override void Update()
    {
        base.Update();
    }
}
