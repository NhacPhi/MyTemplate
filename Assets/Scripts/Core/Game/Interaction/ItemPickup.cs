using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Configuration")]
    [Tooltip("ID của item để nhận dạng (ví dụ dùng cho GameDataBase)")]
    public string itemID;
    
    [Tooltip("Số lượng item nhận được")]
    public int amount = 1;

    /// <summary>
    /// Hàm này được gọi bởi InteractionManager khi người chơi nhấn nút tương tác
    /// </summary>
    public void PickUp()
    {
        GameEvent.OnRequestPickupItem?.Invoke(itemID, amount);
        
        Debug.Log($"[ItemPickup] Đã nhặt item: {itemID} x {amount}");

        // Sau khi nhặt xong, phá hủy object khỏi cảnh
        Destroy(gameObject);
    }
}
