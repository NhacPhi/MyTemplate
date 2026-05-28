using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopProductUI : MonoBehaviour
{
    [SerializeField] private ItemUI itemUI;
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private Image imgCurrency;
    [SerializeField] private Button btnBuy;
    [SerializeField] private GameObject soldOutOverlay;
    [SerializeField] private TextMeshProUGUI txtPurchaseLimit;
    
    private ShopProductConfig currentConfig;
    private Action<ShopProductConfig> onBuyClicked;

    private void Awake()
    {
        if (btnBuy != null)
        {
            btnBuy.onClick.AddListener(OnBtnBuyClicked);
        }
    }

    private void OnDestroy()
    {
        if (btnBuy != null)
        {
            btnBuy.onClick.RemoveAllListeners();
        }
    }

    public void Setup(ShopProductConfig config, Rare itemRare, Sprite itemSprite, Sprite itemBackground, Sprite currencySprite, Action<ShopProductConfig> onBuyCallback)
    {
        currentConfig = config;
        onBuyClicked = onBuyCallback;

        if (itemUI != null)
        {
            itemUI.Init(config.ReferenceID, itemRare, itemSprite, itemBackground, config.ItemAmount);
        }
        
        if (imgCurrency != null && currencySprite != null)
        {
            imgCurrency.sprite = currencySprite;
            imgCurrency.gameObject.SetActive(true);
        }
        else if (imgCurrency != null)
        {
            imgCurrency.gameObject.SetActive(false);
        }
        
        if (txtPrice != null) txtPrice.text = config.Price.ToString();
        
        if (soldOutOverlay != null)
        {
            soldOutOverlay.SetActive(false); // Can be updated externally based on limits
        }
    }
    
    public void SetName(string name)
    {
        if (txtName != null) txtName.text = name;
    }

    public void SetShardStatus(bool isShard)
    {
        if (itemUI != null) itemUI.ActiveFragIcon(isShard);
    }

    public void SetSoldOutStatus(bool isSoldOut)
    {
        if (soldOutOverlay != null)
        {
            soldOutOverlay.SetActive(isSoldOut);
        }
        
        if (btnBuy != null)
        {
            btnBuy.interactable = !isSoldOut;
        }
    }

    public void SetPurchaseLimit(int currentPurchase, int limitCount)
    {
        if (txtPurchaseLimit != null)
        {
            // Nếu limit_count = -1 (không giới hạn) hoặc <= 0 thì ẩn text đi (tuỳ logic game của bạn)
            if (limitCount < 0)
            {
                txtPurchaseLimit.gameObject.SetActive(false);
            }
            else
            {
                txtPurchaseLimit.gameObject.SetActive(true);
                string format = LocalizationManager.Instance.GetLocalizedValue("STR_NUMBER_OF_PURCHASE");
                txtPurchaseLimit.text = string.Format(format, currentPurchase, limitCount);
            }
        }
    }

    private void OnBtnBuyClicked()
    {
        onBuyClicked?.Invoke(currentConfig);
    }
}
