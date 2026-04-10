using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using UnityEngine.UI;
using TMPro;

public class CharacterAscentionUpgrade : MonoBehaviour
{
    public ItemUI prefabUI;

    [SerializeField] private GameObject content;

    [SerializeField] private TextMeshProUGUI txtCoinNeededToUpgrade;
    [SerializeField] private Button btnAscentionUgpgrade;

    private string currentCharacter = string.Empty;
    int nextTier = 0;
    int requiredLevel = 0;
    List<CostIteam> requiredItem;
    int requiredCoin = 0;

    [Inject] private InventoryManager inventory;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private PlayerCharacterManager playerCharacterManager;

    private void OnEnable()
    {
        btnAscentionUgpgrade.onClick.AddListener(OnBtnAscendClicked);
    }

    private void OnDisable()
    {
        btnAscentionUgpgrade.onClick.RemoveAllListeners();
    }

    private void OnBtnAscendClicked()
    {
        if (string.IsNullOrEmpty(currentCharacter)) return;

        var upgrader = playerCharacterManager.GetUpgradeManager(currentCharacter);
        if (upgrader != null)
        {
            bool success = upgrader.Ascend();
            if (success)
            {
                // Cập nhật lại UI thông qua event sau khi đột phá thành công
                UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
            }
        }
    }

    public void CharacterAscentionUpdate(string id)
    {
        currentCharacter = id;
        CharacterConfig config = gameDataBase.GetCharacterConfig(id);
        CharacterSaveData data = playerCharacterManager.GetCharacter(id).SaveData;

        txtCoinNeededToUpgrade.text = Utility.FormatCurrency(requiredCoin);

        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var cost in requiredItem)
        {
            ItemUI itemUI = Instantiate(prefabUI, content.transform);
            var itemConfig = gameDataBase.GetItemConfig(cost.ID);
            int ownQuantity = inventory.GetItemQuantity(cost.ID);
            itemUI.InitRequirement(cost.ID, itemConfig.Rarity, itemConfig.Icon, gameDataBase.GetBGItemByRare(itemConfig.Rarity), ownQuantity, cost.Quantity);
        }
    }

    public bool IsShowCharactterAscentionUpgrade(string id)
    {
        var upgrader = playerCharacterManager.GetUpgradeManager(id);
        return upgrader.GetNextAscensionRequirements(out nextTier, out requiredLevel, out requiredItem, out requiredCoin);
    }
}
