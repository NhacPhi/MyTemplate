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
            HandleTurn(enemy);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Major Skill");
            var enemy = target.gameObject.GetComponent<Entity>();
            gameObject.GetComponent<EntitySkill>().ExecuteMainSkill(Skill.Major);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Ultimate Skill");
            var enemy = target.gameObject.GetComponent<Entity>(); 
            gameObject.GetComponent<EntitySkill>().ExecuteMainSkill(Skill.Main);
            HandleTurn(enemy);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Take Hit");
            entityStateData.StateManager.ChangeState(EntityState.HIT);
        }
    }
}
