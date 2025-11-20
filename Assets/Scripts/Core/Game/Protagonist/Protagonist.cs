using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Protagonist : MonoBehaviour
{
    [SerializeField] private TransformAnchor gameplayCameraTransform = default;
    [SerializeField] private TransformAnchor playerTranform = default;

    [SerializeField] private GameObject objPlayer;

    [SerializeField] private SpriteRenderer character;
    [SerializeField] private SpriteRenderer clone;

    [SerializeField] private Animator smoke;

    [SerializeField] private float velocity = 0;
    [SerializeField] private float timeToGetWeapon;

    float countDown = 0;

    [SerializeField] private WeaponController weapon;

    private Vector3 moveVector = Vector3.zero;

    private bool equipWeapon = false;
    private bool isClone = false;

    private void OnEnable()
    {
        GameEvent.OnPlayerMove += PlayerMovement;
        GameEvent.OnPlayerAttack += PlayerAttack;
        GameEvent.OnPlayerTransform += Transformation;
        playerTranform.Provide(transform);
    }

    private void OnDisable()
    {
        GameEvent.OnPlayerMove -= PlayerMovement;
        GameEvent.OnPlayerAttack -= PlayerAttack;
        GameEvent.OnPlayerTransform -= Transformation;
    }
    // Start is called before the first frame update
    void Update()
    {
        if (equipWeapon && countDown > 0)
        {
            countDown -= Time.deltaTime;
            if (countDown < 0)
            {
                equipWeapon = false;
                weapon.TakeOffWeapon();
            }
        }
    }

    private void PlayerMovement(Vector2 vec)
    {
        if (vec.x > 0)
        {
            //characterPlayer.flipX = true;
            objPlayer.transform.localScale = new Vector3(-1,1,1);
        }
        else
        {
            //characterPlayer.flipX = false;
            objPlayer.transform.localScale = new Vector3(1, 1, 1);
        }
        
        moveVector.x = vec.x;
        moveVector.z = vec.y;

        transform.position = transform.position + moveVector * velocity * Time.deltaTime;

    }

    private void PlayerAttack()
    {
        if (!equipWeapon && !isClone)
        {
            weapon.WeaponDoSomething(1);
            equipWeapon = true;
            countDown = timeToGetWeapon;

        }
        else if(equipWeapon)
        {
            countDown = timeToGetWeapon;
            weapon.WeaponDoSomething(2);
        }
        
    }

    private void Transformation()
    {
        if (equipWeapon)
        {
            equipWeapon = false;
            weapon.TakeOffWeapon();
        }

        smoke.SetTrigger("Start");
        character.gameObject.SetActive(isClone);
        clone.gameObject.SetActive(!isClone);
        isClone = !isClone;
    }

}
