using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }


    public void WeaponDoSomething(int value)
    {
        if(value == 1)
        {
            animator.SetTrigger("GetWeapon");
        }
        else
        {
            animator.SetTrigger("Attack");
        }
    }

    public void TakeOffWeapon()
    {
        animator.SetTrigger("TakeOffWeapon");
    }

    public void ActiveWeapon()
    {
        //gameObject.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
        //Debug.Log("ATTACK");
        gameObject.SetActive(true);
    }
    
}
