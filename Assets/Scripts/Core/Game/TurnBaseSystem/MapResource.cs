using UnityEngine;
using System;

[DisallowMultipleComponent]
public class MapResource : MonoBehaviour
{
    [Header("Resource Identity")]
    [Tooltip("Unique ID của vật thể. Không được trùng lặp. Sẽ tự sinh bởi MapResourceEditor.")]
    public string UniqueID;

    [Header("Respawn Settings")]
    public RespawnType RespawnRule = RespawnType.DailyReset;
    
    [Tooltip("Chỉ có tác dụng nếu RespawnRule = TimeBased")]
    public float CooldownMinutes = 5f;

    [Header("Debug")]
    [SerializeField, ReadOnly] private bool _isConsumed = false;

    private void Start()
    {
        CheckRespawn();
    }

    private void CheckRespawn()
    {
        if (string.IsNullOrEmpty(UniqueID))
        {
            Debug.LogWarning($"[MapResource] {gameObject.name} không có UniqueID. Bỏ qua check hồi sinh.");
            return;
        }

        bool isReady = true;

        if (GameEvent.OnCheckRespawnStatus != null)
        {
            isReady = GameEvent.OnCheckRespawnStatus.Invoke(UniqueID, RespawnRule, CooldownMinutes);
        }

        if (!isReady)
        {
            // Chưa tới thời gian hồi sinh -> ẩn vật thể đi
            _isConsumed = true;
            gameObject.SetActive(false);
        }
        else
        {
            _isConsumed = false;
        }
    }

    /// <summary>
    /// Gọi hàm này khi quái bị tiêu diệt hoặc rương/vật phẩm bị nhặt.
    /// </summary>
    public void ConsumeResource()
    {
        if (_isConsumed) return;
        _isConsumed = true;

        if (!string.IsNullOrEmpty(UniqueID) && RespawnRule != RespawnType.OnSceneReload)
        {
            GameEvent.OnRecordResourceDestroyed?.Invoke(UniqueID);
        }
    }
}


