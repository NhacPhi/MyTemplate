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
            HandleTurn(target.gameObject.GetComponent<Entity>());
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Major Skill");

        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Ultimate Skill");

        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Take Hit");
            entityStateData.StateManager.ChangeState(EntityState.HIT);
        }
    }
}
