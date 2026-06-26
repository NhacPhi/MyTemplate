using UnityEngine;
using VContainer;

public class PickupNotificationManager : MonoBehaviour
{
    [Inject] private GameDataBase gameDataBase;
    
    [SerializeField] private ItemPickupNotification notificationPrefab;
    [SerializeField] private Transform container; // Kéo thả Object có Vertical Layout Group vào đây

    private void OnEnable()
    {
        GameEvent.OnRequestPickupItem += ShowNotification;
    }

    private void OnDisable()
    {
        GameEvent.OnRequestPickupItem -= ShowNotification;
    }

    private void ShowNotification(string itemID, int amount)
    {
        var config = gameDataBase.GetItemConfig(itemID);
        if (config != null && notificationPrefab != null && container != null)
        {
            var notification = Instantiate(notificationPrefab, container);
            notification.Setup(config, amount);
        }
    }
}
