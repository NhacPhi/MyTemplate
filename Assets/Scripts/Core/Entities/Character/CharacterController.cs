using MathNet.Numerics.Statistics.Mcmc;
using System.Collections;
using System.Collections.Generic;
using Tech.Composite;
using UnityEngine;

public class CharacterController : Entity
{
    //[SerializeField] private HealthBar healthUI;

    private void Awake()
    {

    }


    protected override void Update()
    {
        base.Update();
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Base Attack");
            var enemy = target.gameObject.GetComponent<Entity>();
            //gameObject.GetComponent<EntitySkill>().ExecuteMainSkill(SkillCharacter.Base);
            HandleTurn(enemy, false);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Major SkillCharacter");
            var enemy = target.gameObject.GetComponent<Entity>();
            gameObject.GetComponent<EntitySkill>().ExecuteMainSkill(SkillCharacter.Major);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Ultimate SkillCharacter");
            var enemy = target.gameObject.GetComponent<Entity>(); 
            gameObject.GetComponent<EntitySkill>().ExecuteMainSkill(SkillCharacter.Ultimate);
            HandleTurn(enemy, true);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Take Hit");
            entityStateData.StateManager.ChangeState(EntityState.HIT);
        }
    }
}
