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

        // Tiêu thụ tài nguyên trên bản đồ nếu có MapResource
        MapResource mapResource = GetComponent<MapResource>();
        if (mapResource != null)
        {
            mapResource.ConsumeResource();
            gameObject.SetActive(false);
        }
        else
        {
            // Fallback nếu không có hệ thống Respawn
            Destroy(gameObject);
        }
    }
}
