using SixLabors.ImageSharp;
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

    public LayerMask groundLayer;

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

    private void PlayerMovement(Vector2 input)
    {
        if(input.magnitude > 0)
        {
            if (input.x > 0) objPlayer.transform.localScale = new Vector3(-1, 1, 1);
            else if (input.x < 0) objPlayer.transform.localScale = new Vector3(1, 1, 1);

            Vector3 moveDir = new Vector3(input.x, 0, input.y).normalized;

            //transform.position = transform.position + moveDir * velocity * Time.deltaTime;

            Vector3 nextPosition = transform.position + moveDir * velocity * Time.deltaTime;

            if (Physics.Raycast(nextPosition - Vector3.forward, Vector3.down, 5f, groundLayer))
            {
                Debug.DrawRay(nextPosition, Vector3.down * 5, UnityEngine.Color.red);
                transform.position = nextPosition;
            }
        }

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
