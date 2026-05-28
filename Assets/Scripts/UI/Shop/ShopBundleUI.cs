using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class BundleContentUIInfo
{
    public string ItemID;
    public int Amount;
    public Rare ItemRare;
    public Sprite ItemSprite;
    public Sprite ItemBackground;
    public bool IsShard;
}

public class ShopBundleUI : MonoBehaviour
{
    [Header("Item Slots (Max 4)")]
    [SerializeField] private ItemUI[] itemSlots;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private Image imgCurrency;
    [SerializeField] private Button btnBuy;
    [SerializeField] private GameObject soldOutOverlay;
    [SerializeField] private TextMeshProUGUI txtPurchaseLimit;
    
    private ShopProductConfig currentConfig;
    private Action<ShopProductConfig> onBuyClicked;

    private Vector2[] originalPositions;

    private void Awake()
    {
        if (btnBuy != null)
        {
            btnBuy.onClick.AddListener(OnBtnBuyClicked);
        }

        InitOriginalPositions();
    }

    private void InitOriginalPositions()
    {
        if (itemSlots != null && originalPositions == null)
        {
            if (itemSlots.Length > 0 && itemSlots[0] != null && itemSlots[0].transform.parent != null)
            {
                // Bật tất cả các item lên để GridLayoutGroup tính toán chuẩn
                for (int i = 0; i < itemSlots.Length; i++)
                {
                    if (itemSlots[i] != null) itemSlots[i].gameObject.SetActive(true);
                }
                
                // Ép GridLayoutGroup tính toán vị trí ngay lập tức trước khi cache
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(itemSlots[0].transform.parent.GetComponent<RectTransform>());
            }

            originalPositions = new Vector2[itemSlots.Length];
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (itemSlots[i] != null)
                {
                    RectTransform rt = itemSlots[i].GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        originalPositions[i] = rt.anchoredPosition;
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (btnBuy != null)
        {
            btnBuy.onClick.RemoveAllListeners();
        }
    }

    public void Setup(ShopProductConfig config, List<BundleContentUIInfo> contents, Sprite currencySprite, Action<ShopProductConfig> onBuyCallback)
    {
        currentConfig = config;
        onBuyClicked = onBuyCallback;

        // Setup Item Slots
        if (itemSlots != null)
        {
            InitOriginalPositions();
            // Reset về vị trí ban đầu trước khi setup
            RestoreOriginalPositions();

            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (itemSlots[i] == null) continue;

                if (i < contents.Count)
                {
                    itemSlots[i].gameObject.SetActive(true);
                    itemSlots[i].Init(contents[i].ItemID, contents[i].ItemRare, contents[i].ItemSprite, contents[i].ItemBackground, contents[i].Amount);
                    itemSlots[i].ActiveFragIcon(contents[i].IsShard);
                }
                else
                {
                    itemSlots[i].gameObject.SetActive(false);
                }
            }
            
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
            soldOutOverlay.SetActive(false);
        }
    }

    private void RestoreOriginalPositions()
    {
        if (itemSlots == null || originalPositions == null) return;
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null)
            {
                RectTransform rt = itemSlots[i].GetComponent<RectTransform>();
                if (rt != null && i < originalPositions.Length)
                {
                    rt.anchoredPosition = originalPositions[i];
                }
            }
        }
    }


    
    // Hàm layout cải tiến
    public void ApplyPyramidLayout()
    {
        InitOriginalPositions();

        // 1 Item ở giữa trên, 2 Item ở dưới
        // Lấy tọa độ từ các slot ban đầu để tính toán
        if (itemSlots != null && itemSlots.Length >= 3 && originalPositions != null && originalPositions.Length >= 4)
        {
            RectTransform rt0 = itemSlots[0].GetComponent<RectTransform>();
            RectTransform rt1 = itemSlots[1].GetComponent<RectTransform>();
            RectTransform rt2 = itemSlots[2].GetComponent<RectTransform>();

            // Tính y của hàng trên và hàng dưới
            float topY = originalPositions[0].y;
            float bottomY = originalPositions[2].y;
            
            // Tính x của cột trái và cột phải
            float leftX = originalPositions[0].x;
            float rightX = originalPositions[1].x;
            float centerX = (leftX + rightX) / 2f;

            if (rt0 != null) rt0.anchoredPosition = new Vector2(centerX, topY);
            if (rt1 != null) rt1.anchoredPosition = new Vector2(leftX, bottomY);
            if (rt2 != null) rt2.anchoredPosition = new Vector2(rightX, bottomY);
            
            // Nếu có LayoutGroup thì tắt đi để nó không ghi đè vị trí
            if (rt0 != null && rt0.parent != null)
            {
                UnityEngine.UI.LayoutGroup layoutGroup = rt0.parent.GetComponent<UnityEngine.UI.LayoutGroup>();
                if (layoutGroup != null)
                {
                    layoutGroup.enabled = false;
                }
            }
        }
    }

    public void SetupLayout(int count)
    {
        if (itemSlots != null && itemSlots.Length > 0 && itemSlots[0] != null && itemSlots[0].transform.parent != null)
        {
            UnityEngine.UI.LayoutGroup layoutGroup = itemSlots[0].transform.parent.GetComponent<UnityEngine.UI.LayoutGroup>();
            if (layoutGroup != null)
            {
                // Bật LayoutGroup cho 4 items, tắt cho 3 items
                layoutGroup.enabled = count != 3;
            }
        }

        if (count == 3)
        {
            ApplyPyramidLayout();
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

    private void OnBtnBuyClicked()
    {
        onBuyClicked?.Invoke(currentConfig);
    }

#if UNITY_EDITOR
    [ContextMenu("Preview Layout: 3 Items (Pyramid)")]
    public void PreviewLayout3Items()
    {
        if (itemSlots == null || itemSlots.Length < 4) return;
        
        InitOriginalPositions();
        
        if (itemSlots[0] != null && itemSlots[0].transform.parent != null)
        {
            UnityEngine.UI.LayoutGroup layoutGroup = itemSlots[0].transform.parent.GetComponent<UnityEngine.UI.LayoutGroup>();
            if (layoutGroup != null) layoutGroup.enabled = false;
        }

        ApplyPyramidLayout();
        
        // Giả lập trạng thái bật/tắt của 3 items
        if (itemSlots[0] != null) itemSlots[0].gameObject.SetActive(true);
        if (itemSlots[1] != null) itemSlots[1].gameObject.SetActive(true);
        if (itemSlots[2] != null) itemSlots[2].gameObject.SetActive(true);
        if (itemSlots[3] != null) itemSlots[3].gameObject.SetActive(false);
    }

    [ContextMenu("Preview Layout: 4 Items (Grid)")]
    public void PreviewLayout4Items()
    {
        if (itemSlots == null) return;
        
        InitOriginalPositions();
        RestoreOriginalPositions();
        
        if (itemSlots.Length > 0 && itemSlots[0] != null && itemSlots[0].transform.parent != null)
        {
            UnityEngine.UI.LayoutGroup layoutGroup = itemSlots[0].transform.parent.GetComponent<UnityEngine.UI.LayoutGroup>();
            if (layoutGroup != null) layoutGroup.enabled = true;
        }

        // Bật tất cả 4 items
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] != null) itemSlots[i].gameObject.SetActive(true);
        }
    }
#endif
}
