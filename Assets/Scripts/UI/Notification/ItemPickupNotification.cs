using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class ItemPickupNotification : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI amountText;

    public void Setup(ItemConfig config, int amount)
    {
        if (config.Icon != null)
        {
            icon.sprite = config.Icon;
        }
        
        string localizedName = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        if (string.IsNullOrEmpty(localizedName))
        {
            localizedName = config.Name.ToString(); // Fallback
        }
        itemNameText.text = localizedName;
        
        amountText.text = $"+{amount}";
        
        HideAfterDelay().Forget();
    }

    private async UniTaskVoid HideAfterDelay()
    {
        await UniTask.Delay(2000); // Đợi 2 giây
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
