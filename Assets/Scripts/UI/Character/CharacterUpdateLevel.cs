using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterUpdateLevel : MonoBehaviour
{
    [Inject] private InventoryManager inventory;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private PlayerCharacterManager playerCharacterManager;


    [SerializeField] private List<ItemUI> exps;
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private TextMeshProUGUI txtCoinUpdateLv;

    [SerializeField] private Slider sliderExp;

    [SerializeField] private Button btnUpdate;
    [SerializeField] private Button btnUpdateTo;
    [SerializeField] private TextMeshProUGUI txtUpdateLv;

    private readonly string[] EXP_ITEM_IDS = { "common_exp", "fine_exp", "rare_exp", "supreme_exp" };
    private string currentCharacter = string.Empty;
    private void OnEnable()
    {
        btnUpdate.onClick.AddListener(OnBtnUpdateClicked);
        btnUpdateTo.onClick.AddListener(OnBtnUpdateToClicked);
    }

    private void OnDisable()
    {
        btnUpdate.onClick.RemoveAllListeners();
        btnUpdateTo.onClick.RemoveAllListeners();
    }

    private void OnBtnUpdateClicked()
    {
        if (string.IsNullOrEmpty(currentCharacter)) return;

        var character = playerCharacterManager.GetCharacter(currentCharacter);
        var upgrader = playerCharacterManager.GetUpgradeManager(currentCharacter);

        // Khóa nút để tránh spam click trong lúc đang chạy animation
        btnUpdate.interactable = false;
        btnUpdateTo.interactable = false;

        // Animate progress bar lên đầy trước, sau đó thực hiện upgrade
        AnimateProgress(() =>
        {
            // Toàn bộ khối này sẽ chạy SAU 0.5 giây
            upgrader.AutoLevelUp(character.SaveData.Level + 1);

            RefreshUI();
            // Bạn có thể cần gọi thêm hàm này để cập nhật lại Text Coin và Level max
            UpdateCharacterUpdateLevel(currentCharacter);

            UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);

            // Mở khóa nút lại
            btnUpdate.interactable = true;
            btnUpdateTo.interactable = true;
        });
    }

    private void OnBtnUpdateToClicked()
    {
        if (string.IsNullOrEmpty(currentCharacter)) return;

        var character = playerCharacterManager.GetCharacter(currentCharacter);
        var upgrader = playerCharacterManager.GetUpgradeManager(currentCharacter);
        int maxLevel = upgrader.GetMaxReachableLevelWithCurrentExpItemsAndCoin();

        if (maxLevel > character.SaveData.Level)
        {
            btnUpdate.interactable = false;
            btnUpdateTo.interactable = false;

            AnimateProgress(() =>
            {
                // Chạy sau 0.5 giây
                upgrader.AutoLevelUp(maxLevel);

                RefreshUI();
                UpdateCharacterUpdateLevel(currentCharacter);

                UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);

                btnUpdate.interactable = true;
                btnUpdateTo.interactable = true;
            });
        }
    }

    public void UpdateCharacterUpdateLevel(string id)
    {
        currentCharacter = id;
        var character = playerCharacterManager.GetCharacter(id);
        var upgrader  = playerCharacterManager.GetUpgradeManager(id);
        var data = character.SaveData;

        int currentCoinData = Utility.GetCoinNeedToUpgradeCacultivate(data.Level);
        int nextCoinData    = Utility.GetCoinNeedToUpgradeCacultivate(data.Level + 1);
        txtCoin.text = Utility.FormatCurrency(nextCoinData - currentCoinData);

        int maxLevel = upgrader.GetMaxReachableLevelWithCurrentExpItemsAndCoin();
        txtUpdateLv.text = $"{LocalizationManager.Instance.GetLocalizedValue("UI_UPGRADE_MAX_LEVEL")} {maxLevel}";

        int maxCoinData = Utility.GetCoinNeedToUpgradeCacultivate(maxLevel);
        txtCoinUpdateLv.text = Utility.FormatCurrency(maxCoinData - currentCoinData);
    }

    public void RefreshUI()
    {
        for (int i = 0; i < EXP_ITEM_IDS.Length; i++)
        {
            string expItemID = EXP_ITEM_IDS[i];
            var expConfig = gameDataBase.GetItemConfig(expItemID);
            var quantity = inventory.GetItemQuantity(expItemID);

            exps[i].Init(expItemID, expConfig.Rarity, expConfig.Icon, gameDataBase.GetBGItemByRare(expConfig.Rarity), quantity);
            exps[i].CanClick = false;
        }
    }
    private Tween _progressTween;

    /// <summary>
    /// Animate progress bar từ 0 lên 1 (đầy) khi bấm nút Upgrade.
    /// Sau khi animation xong sẽ gọi onComplete callback.
    /// </summary>
    private void AnimateProgress(System.Action onComplete)
    {
        _progressTween?.Kill();

        if (sliderExp != null)
        {
            sliderExp.value = 0f;
            _progressTween = sliderExp.DOValue(1f, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }
        else
        {
            onComplete?.Invoke();
        }
    }
}
