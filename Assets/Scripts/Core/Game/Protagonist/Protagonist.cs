using SixLabors.ImageSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameplay.Common;

public class Protagonist : MonoBehaviour, IDamageable
{
    [Header("Player Stats")]
    public int Level = 1;
    public int MaxHP;
    public int CurrentHP { get; private set; }
    public int AttackDamage;

    [SerializeField] private TransformAnchor gameplayCameraTransform = default;
    [SerializeField] private TransformAnchor playerTranform = default;

    [SerializeField] private GameObject objPlayer;

    [SerializeField] private SpriteRenderer character;
    [SerializeField] private SpriteRenderer clone;

    [SerializeField] private Animator smoke;

    [SerializeField] private float velocity = 0;
    [SerializeField] private float timeToGetWeapon;

    [Header("Transformation Settings")]
    [SerializeField] private Collider playerCollider; // Collider cần bật/tắt
    [SerializeField] private float cloneSpeedMultiplier = 1.5f; // Hệ số buff tốc độ
    private float baseVelocity;

    float countDown = 0;

    [SerializeField] private WeaponController weapon;

    private Vector3 moveVector = Vector3.zero;

    private bool equipWeapon = false;
    private bool isClone = false;

    public LayerMask groundLayer;

    private Vector2 movement;

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
        if (CurrentHP <= 0) return; // Nếu đã chết thì không làm gì

        if (equipWeapon && countDown > 0)
        {
            countDown -= Time.deltaTime;
            if (countDown < 0)
            {
                equipWeapon = false;
                weapon.TakeOffWeapon();
            }
        }

#if UNITY_EDITOR
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Hỗ trợ phím cứng WASD phòng khi Input Manager chưa setup
        if (movement.sqrMagnitude == 0)
        {
            if (Input.GetKey(KeyCode.A)) movement.x = -1;
            if (Input.GetKey(KeyCode.D)) movement.x = 1;
            if (Input.GetKey(KeyCode.W)) movement.y = 1;
            if (Input.GetKey(KeyCode.S)) movement.y = -1;
        }

        if(movement.magnitude > 0)
        {
            PlayerMovement(movement);
        }

        // Thêm phím test tấn công (Space / J)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.J))
        {
            PlayerAttack();
        }

        // Thêm phím test biến hình (F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            Transformation();
        }
#endif
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

            // Dùng SphereCast (bắn tia hình cầu có độ dày 0.4f) thay cho Raycast (tia siêu mỏng)
            // Việc này giúp nhân vật không bị lọt tia check xuống khe nứt và đi lướt qua rãnh nhỏ
            Vector3 origin = nextPosition - Vector3.forward + Vector3.up * 0.5f; 
            if (Physics.SphereCast(origin, 0.4f, Vector3.down, out RaycastHit hit, 5f, groundLayer))
            {
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
        
        isClone = !isClone; // Đảo trạng thái trước
        
        character.gameObject.SetActive(!isClone);
        clone.gameObject.SetActive(isClone);
        
        // Bật/Tắt va chạm (deactivate collision)
        if (playerCollider != null)
        {
            playerCollider.enabled = !isClone;
        }

        // Buff thêm tốc độ di chuyển
        if (isClone)
        {
            velocity = baseVelocity * cloneSpeedMultiplier;
        }
        else
        {
            velocity = baseVelocity;
        }
    }

    public void UpdateStats()
    {
        MaxHP = 100 + (Level * 20);
        AttackDamage = 40 + (Level * 5);
        CurrentHP = MaxHP;
        UIEvent.OnUpdatePlayerHP?.Invoke(CurrentHP, MaxHP);
    }

    private void Start()
    {
        UpdateStats();
        baseVelocity = velocity; // Lưu tốc độ cơ bản

        // Tự động tìm Collider nếu bạn quên chưa kéo vào Inspector
        if (playerCollider == null) playerCollider = GetComponent<Collider>();
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        Debug.Log($"Protagonist HP: {CurrentHP}/{MaxHP}");
        UIEvent.OnUpdatePlayerHP?.Invoke(CurrentHP, MaxHP);

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Protagonist Died!");
        // Sử dụng hệ thống load scene của game thay vì SceneManager mặc định
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.RestartCurrentScene();
        }
    }


}
