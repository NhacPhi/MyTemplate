using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopPromotionUI : MonoBehaviour
{
    [Header("Item Info")]
    [SerializeField] private ItemUI itemUI;
    [SerializeField] private TextMeshProUGUI txtName;
    
    [Header("Pricing")]
    [SerializeField] private TextMeshProUGUI txtPrice; // Giá sau khi giảm
    [SerializeField] private TextMeshProUGUI txtOriginalPrice; // Giá gốc (thường gạch ngang)
    [SerializeField] private TextMeshProUGUI txtDiscountLabel; // Nhãn hiển thị % giảm giá (vd: -20%)
    [SerializeField] private Image imgCurrency;
    
    [Header("Status")]
    [SerializeField] private TextMeshProUGUI txtTimer; // Thời gian còn lại
    [SerializeField] private TextMeshProUGUI txtPurchaseLimit;
    [SerializeField] private GameObject soldOutOverlay;
    
    [Header("Interaction")]
    [SerializeField] private Button btnBuy;
    
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

    public void Setup(ShopProductConfig config, Rare itemRare, Sprite itemSprite, Sprite itemBg, Sprite currencySprite, Action<ShopProductConfig> onBuyCallback)
    {
        currentConfig = config;
        onBuyClicked = onBuyCallback;

        if (itemUI != null)
        {
            itemUI.Init(config.ReferenceID, itemRare, itemSprite, itemBg, config.ItemAmount);
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
        
        // Setup Prices
        if (txtPrice != null) txtPrice.text = config.Price.ToString();
        
        if (txtOriginalPrice != null)
        {
            if (config.OriginalPrice > config.Price)
            {
                txtOriginalPrice.text = config.OriginalPrice.ToString();
                txtOriginalPrice.gameObject.SetActive(true);
            }
            else
            {
                txtOriginalPrice.gameObject.SetActive(false); // Ẩn nếu không có giảm giá
            }
        }
        
        // Setup Discount Label
        if (txtDiscountLabel != null)
        {
            if (config.OriginalPrice > config.Price)
            {
                int discountPercent = Mathf.RoundToInt((1f - (config.Price / config.OriginalPrice)) * 100f);
                txtDiscountLabel.text = $"-{discountPercent}%";
                txtDiscountLabel.transform.parent.gameObject.SetActive(true); // Bật cả cái nền của nhãn giảm giá (nếu có)
            }
            else
            {
                txtDiscountLabel.transform.parent.gameObject.SetActive(false);
            }
        }

        if (soldOutOverlay != null)
        {
            soldOutOverlay.SetActive(false);
        }
    }
    
    public void SetName(string name)
    {
        if (txtName != null)
        {
            txtName.text = name;
        }
    }

    public void SetShardStatus(bool isShard)
    {
        if (itemUI != null)
        {
            itemUI.ActiveFragIcon(isShard);
        }
    }

    public void SetPurchaseLimit(int currentPurchase, int limitCount)
    {
        if (txtPurchaseLimit != null)
        {
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
    
    // Bạn có thể gọi hàm này trong Update() từ Panel quản lý hoặc dùng Coroutine để đếm ngược
    public void UpdateTimer(string timeString)
    {
        if (txtTimer != null)
        {
            txtTimer.text = timeString;
        }
    }

    private void OnBtnBuyClicked()
    {
        if (currentConfig != null)
        {
            onBuyClicked?.Invoke(currentConfig);
        }
    }
}
