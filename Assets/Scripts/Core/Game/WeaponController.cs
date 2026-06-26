using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Common;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Hitbox Settings")]
    [SerializeField] private Collider weaponCollider; // Kéo BoxCollider của vũ khí vào đây
    [SerializeField] private Protagonist protagonist; // Kéo nhân vật chính vào đây để lấy Damage và tránh tự chém mình

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (weaponCollider == null) weaponCollider = GetComponent<Collider>();
        
        // Mặc định luôn tắt Hitbox để tránh bug chém trúng quái khi đang đi bộ
        if (weaponCollider != null) weaponCollider.enabled = false;
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
    
    // --- CÁC HÀM NÀY DÀNH CHO ANIMATION EVENT ---
    
    // Đặt event gọi hàm này ở Frame mà lưỡi kiếm bắt đầu bổ xuống
    public void EnableWeaponHitbox()
    {
        if (weaponCollider != null) weaponCollider.enabled = true;
    }

    // Đặt event gọi hàm này ở Frame mà lưỡi kiếm vung xong
    public void DisableWeaponHitbox()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    // Hàm tự động kích hoạt khi có object lọt vào vũ khí (nhớ tick IsTrigger trên Collider)
    private void OnTriggerEnter(Collider other)
    {
        // Bỏ qua nếu lỡ chém trúng chính bản thân nhân vật (Protagonist)
        if (protagonist != null && other.GetComponent<Protagonist>() != null) return;

        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // Lấy lượng Damage từ Protagonist
            int damage = protagonist != null ? protagonist.AttackDamage : 10;
            damageable.TakeDamage(damage);
            Debug.Log($"<color=orange>Player hit {other.name} for {damage} damage!</color>");
            
            // Nếu bạn muốn 1 lần vung kiếm chỉ chém 1 quái, bạn có thể Disable Hitbox ngay tại đây:
            // DisableWeaponHitbox(); 
        }
    }
}
