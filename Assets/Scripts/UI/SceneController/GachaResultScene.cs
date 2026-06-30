using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;
using VContainer;
using DG.Tweening;
using System;

public class GachaResultScene : WindowController
{
    [Header("UI References")]
    [SerializeField] private GachaResultCardUI characterCardPrefab;
    [SerializeField] private GachaResultCardUI weaponCardPrefab;
    [SerializeField] private Transform resultItemsContainer;
    
    [Header("Buttons")]
    [SerializeField] private Button btnRollAgain;
    [SerializeField] private Button btnClose;
    [SerializeField] private TextMeshProUGUI txtRollAgainCost;

    [Inject] private UIManager uiManager;
    [Inject] private GameDataBase db;
    [Inject] private CurrencyManager currencyManager;
    [Inject] private GachaManager gachaManager;

    private void Start()
    {
        btnRollAgain.onClick.AddListener(OnRollAgainClicked);
        btnClose.onClick.AddListener(OnCloseClicked);

        PopulateResults();
    }

    private void OnEnable()
    {
        PopulateResults();
        UpdateRollAgainButtonText();
    }

    private void PopulateResults()
    {
        if (resultItemsContainer == null || characterCardPrefab == null || weaponCardPrefab == null)
        {
            Debug.LogWarning("[GachaResultScene] Missing container or prefabs.");
            return;
        }

        // Xóa các UI Item cũ nếu có
        foreach (Transform child in resultItemsContainer)
        {
            child.DOKill();
            Destroy(child.gameObject);
        }

        // 2. Instantiate and setup new result items
        List<GachaItemResult> results = GachaRollState.LastResults;
        if (results == null || results.Count == 0)
        {
            Debug.LogWarning("[GachaResultScene] No roll results found in GachaRollState.");
            return;
        }

        // Tính chiều ngang chính xác từ Prefab
        float charWidth = characterCardPrefab.RectTransform.rect.width > 0 ? characterCardPrefab.RectTransform.rect.width : characterCardPrefab.RectTransform.sizeDelta.x;
        float weaponWidth = weaponCardPrefab.RectTransform.rect.width > 0 ? weaponCardPrefab.RectTransform.rect.width : weaponCardPrefab.RectTransform.sizeDelta.x;

        for (int i = 0; i < results.Count; i++)
        {
            var item = results[i];
            GachaResultCardUI prefabToSpawn = item.isCharacter ? characterCardPrefab : weaponCardPrefab;
            
            float exactCardWidth = item.isCharacter ? charWidth : weaponWidth;
            if (exactCardWidth <= 0) exactCardWidth = 200f; // Phòng hờ nếu prefab lỗi

            GachaResultCardUI card = Instantiate(prefabToSpawn, resultItemsContainer);

            if (db != null && !string.IsNullOrEmpty(item.itemId))
            {
                var itemConfig = db.GetItemConfig(item.itemId);
                if (itemConfig != null)
                {
                    string localizedName = LocalizationManager.Instance.GetLocalizedValue(itemConfig.Name);
                    Sprite displayIcon = itemConfig.Icon;

                    // Nếu là character, ưu tiên dùng BigIcon
                    if (item.isCharacter)
                    {
                        var charConfig = db.GetCharacterConfig(item.itemId);
                        if (charConfig != null && charConfig.BigIcon != null)
                        {
                            displayIcon = charConfig.BigIcon;
                        }
                    }
                    // Nếu là weapon, lấy BigIcon từ component vũ khí
                    else if (itemConfig.Type == ItemType.Weapon && itemConfig.Weapon != null && itemConfig.Weapon.BigIcon != null)
                    {
                        displayIcon = itemConfig.Weapon.BigIcon;
                    }

                    if (item.isConverted)
                    {
                        if (card is GachaResultCharacterCardUI charCard)
                        {
                            charCard.SetShardConversion(true, item.convertedShardAmount);
                        }
                        else
                        {
                            // Fallback if the user hasn't updated the prefab script yet
                            localizedName += $"\n<size=80%>(+{item.convertedShardAmount} mảnh)</size>";
                        }
                    }
                    else if (card is GachaResultCharacterCardUI charCardNormal)
                    {
                        charCardNormal.SetShardConversion(false, 0);
                    }
                    
                    card.Setup(localizedName, displayIcon, item.rarity);
                }
            }
            else
            {
                // Fallback nếu chưa có ID thực tế
                card.Setup(item.itemName, null, item.rarity);
            }

            // --- ANIMATION XÒE BÀI TỪ TRUNG TÂM ---
            RectTransform rect = card.RectTransform;
            
            // Bắt đầu từ gốc ở giữa
            rect.anchoredPosition = new Vector2(0, -100f);
            rect.localScale = Vector3.zero;
            rect.localRotation = Quaternion.Euler(0, 0, 0);

            // Xác định tâm và tính khoảng cách từ tâm
            float centerIndex = (results.Count - 1) / 2f;
            float distFromCenter = i - centerIndex; // Âm = bên trái, Dương = bên phải, 0 = chính giữa

            float xPos = distFromCenter * exactCardWidth;
            float yPos = 0f; // Nằm thẳng hàng ngang
            float delay = i * 0.1f; // Tăng tốc độ xuất hiện một chút

            rect.DOScale(Vector3.one, 0.4f).SetDelay(delay).SetEase(Ease.OutBack).SetUpdate(true).SetLink(rect.gameObject);
            rect.DOAnchorPos(new Vector2(xPos, yPos), 0.5f).SetDelay(delay).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(rect.gameObject);

            // Gắn hiệu ứng 3D Mirror Parallax
            var mirrorEffect = card.gameObject.GetComponent<CardMirrorEffect>();
            if (mirrorEffect == null) mirrorEffect = card.gameObject.AddComponent<CardMirrorEffect>();
            
            // Bắn vệt sáng lấp lánh khi lá bài đã xuất hiện xong
            DG.Tweening.DOVirtual.DelayedCall(delay + 0.3f, () => {
                if (mirrorEffect != null) mirrorEffect.PlayGlareSweep();
            }, ignoreTimeScale: true).SetLink(rect.gameObject);
        }
    }

    private void UpdateRollAgainButtonText()
    {
        if (btnRollAgain == null) return;

        int count = GachaRollState.LastRollCount;
        
        var textComp = btnRollAgain.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null)
        {
            string locKey = "UI_SINGLE_SUMMON";
            if (count == 5) locKey = "UI_SUMMON_5";
            else if (count >= 10) locKey = "UI_SUMMON_10";
            
            textComp.text = LocalizationManager.Instance.GetLocalizedValue(locKey);
        }

        if (txtRollAgainCost != null)
        {
            if (db != null)
            {
                var config = db.GetGachaConfig(GachaRollState.LastBannerType);
                if (config != null)
                {
                    txtRollAgainCost.text = $"x{count}";
                }
            }
            else
            {
                txtRollAgainCost.text = $"x{count}";
            }
        }
    }

    private void OnRollAgainClicked()
    {
        int count = GachaRollState.LastRollCount;
        string bannerId = GachaRollState.LastBannerType;

        if (db != null && currencyManager != null)
        {
            var config = db.GetGachaConfig(bannerId);
            if (config != null)
            {
                // Mỗi lượt quay tương ứng với 1 ticket
                int totalCost = count;
                
                if (System.Enum.TryParse<CurrencyType>(config.Cost.Type, true, out var currencyType))
                {
                    if (currencyManager.GetQuantityCurrecy(currencyType) < totalCost)
                    {
                        // Hiện popup báo thiếu tiền
                        Action confirmAction = () => { uiManager.OpenWindowScene(ScreenIds.ShopScene); };
                        Action cancelAction = () => { /* Đóng popup */ };

                        var popupProps = new ConfirmationPopupProperties(
                            LocalizationManager.Instance.GetLocalizedValue("UI_REMIND"), 
                            LocalizationManager.Instance.GetLocalizedValue("UI_NOT_ENOUGH_RESOURCE"), 
                            LocalizationManager.Instance.GetLocalizedValue("UI_GO_TO_SHOP"), 
                            LocalizationManager.Instance.GetLocalizedValue("UI_CANCEL"), 
                            confirmAction, 
                            cancelAction
                        );
                        uiManager.OpenWindowScene(ScreenIds.PopupConfirm, popupProps);
                        return;
                    }
                    
                    // Trừ tiền
                    currencyManager.Spend(currencyType, totalCost);
                }
            }
        }

        Debug.Log($"[GachaResultScene] Rerolling {count}x on banner {bannerId}.");
        
        if (gachaManager != null)
        {
            var results = gachaManager.RollGacha(bannerId, count);
            GachaRollState.LastResults = results;
        }

        UI_Close();
        if (uiManager != null)
        {
            uiManager.OpenWindowScene(ScreenIds.GachaCutsceneScene);
        }
    }

    private void OnCloseClicked()
    {
        UI_Close();
        if (uiManager != null)
        {
            uiManager.OpenWindowScene(ScreenIds.GachaMainScene);
        }
    }
}
