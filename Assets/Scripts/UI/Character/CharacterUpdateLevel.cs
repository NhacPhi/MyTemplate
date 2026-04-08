using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;
using UnityEngine.UI;

public class CharacterUpdateLevel : MonoBehaviour
{
    [Inject] private InventoryManager inventory;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private PlayerCharacterManager playerCharacterManager;


    [SerializeField] private List<ItemUI> exps;
    [SerializeField] private TextMeshProUGUI txtCoin;
    [SerializeField] private TextMeshProUGUI txtCoinUpdateLv;

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
        character.AutoLevelUp(character.SaveData.Level + 1);
        
        RefreshUI();
        UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
    }

    private void OnBtnUpdateToClicked()
    {
        if (string.IsNullOrEmpty(currentCharacter)) return;

        var character = playerCharacterManager.GetCharacter(currentCharacter);
        int maxLevel = character.GetMaxReachableLevelWithCurrentExpItems();
        
        if (maxLevel > character.SaveData.Level)
        {
            character.AutoLevelUp(maxLevel);
        }
        
        RefreshUI();
        UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
    }

    public void UpdateCharacterUpdateLevel(string id)
    {
        currentCharacter = id;
        var character = playerCharacterManager.GetCharacter(id);
        var data = character.SaveData;

        int currentCoinData = Utility.GetCoinNeedToUpgradeCacultivate(data.Level);
        int nextCoinData = Utility.GetCoinNeedToUpgradeCacultivate(data.Level + 1);
        txtCoin.text = Utility.FormatCurrency(nextCoinData - currentCoinData);

        int maxLevel = character.GetMaxReachableLevelWithCurrentExpItems();
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
}
