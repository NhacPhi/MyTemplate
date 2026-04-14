using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VContainer;
using UnityEngine.UI;

public class WeaponAscendCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private UpgradesUI upgradeUI;

    [SerializeField] private TextMeshProUGUI txtCurentHP;
    [SerializeField] private TextMeshProUGUI txtCurentATK;

    [SerializeField] private TextMeshProUGUI txtNameAndLevel;
    [SerializeField] private TextMeshProUGUI txtUse;
    [SerializeField] private TextMeshProUGUI txtCoin;

    [SerializeField] private Button btnAssended;

    [SerializeField] private GameObject ascendOb;
    [SerializeField] private GameObject reachedOb;

    [Header("Material Selection")]
    [SerializeField] private List<AscendMaterialSlotUI> materialSlots;
    [SerializeField] private AscendMaterialCategoryUI materialCategory;

    [Inject] GameDataBase gameDataBase;
    [Inject] SaveSystem save;
    [Inject] ForgeManager forgeManager;

    private string currentTargetWeaponID;
    //private int currentEditingSlotIndex = -1;

    public List<string> GetSelectedMaterials()
    {
        List<string> materials = new List<string>();
        if (materialSlots != null)
        {
            foreach (var slot in materialSlots)
            {
                if (!string.IsNullOrEmpty(slot.SelectedWeaponUUID))
                    materials.Add(slot.SelectedWeaponUUID);
            }
        }
        return materials;
    }

    private void Awake()
    {
        UIEvent.OnSlelectWeaponEnchance += UpdateWeaponAscendCard;

        if (materialSlots != null)
        {
            for (int i = 0; i < materialSlots.Count; i++)
            {
                int index = i;
                materialSlots[i].OnSlotClicked += () => ToggleSlotMaterial(index);
            }
        }

        if (materialCategory != null)
        {
            materialCategory.OnMaterialSelected += OnMaterialSelected;
        }

        if (btnAssended != null)
        {
            btnAssended.onClick.AddListener(OnBtnAscendClicked);
        }
    }

    private void OnEnable()
    {
        if (materialCategory != null && !string.IsNullOrEmpty(currentTargetWeaponID))
        {
            materialCategory.Init(currentTargetWeaponID);
        }
    }

    private void OnDisable()
    {
        if (materialCategory != null)
        {
            materialCategory.CloseCategory();
        }
    }


    private void OnDestroy()
    {
        UIEvent.OnSlelectWeaponEnchance -= UpdateWeaponAscendCard;

        if (materialSlots != null)
        {
            for (int i = 0; i < materialSlots.Count; i++)
            {
                materialSlots[i].OnSlotClicked = null;
            }
        }

        if (materialCategory != null)
        {
            materialCategory.OnMaterialSelected -= OnMaterialSelected;
        }
    }

    private void ToggleSlotMaterial(int slotIndex)
    {
        var slot = materialSlots[slotIndex];

        // Nếu slot đã có nguyên liệu -> Bỏ chọn (gỡ khỏi slot)
        if (!string.IsNullOrEmpty(slot.SelectedWeaponUUID))
        {
            OnMaterialSelected(slot.SelectedWeaponUUID);
            return;
        }

        // Nếu slot đang trống -> Auto-fill món nguyên liệu hợp lệ đầu tiên chưa được chọn
        if (forgeManager != null)
        {
            var duplicates = forgeManager.GetDuplicateWeapons(currentTargetWeaponID);
            var selected = GetSelectedMaterials();
            foreach (var item in duplicates)
            {
                if (!selected.Contains(item.UUID))
                {
                    OnMaterialSelected(item.UUID);
                    break;
                }
            }
        }
    }

    private void OnBtnAscendClicked()
    {
        if (forgeManager == null || string.IsNullOrEmpty(currentTargetWeaponID)) return;

        var selectedMaterials = GetSelectedMaterials();
        if (selectedMaterials.Count == 0) return;

        foreach (var materialUUID in selectedMaterials)
        {
            // Thực hiện đột phá liên tiếp bằng tất cả phôi đã chọn
            bool success = forgeManager.AscendWeapon(currentTargetWeaponID, materialUUID);
            if (!success)
            {
                // Ngưng nếu có lỗi (chẳng hạn hết tiền giữa chừng)
                break;
            }
        }
    }

    private void OnMaterialSelected(string materialUUID)
    {
        if (materialSlots == null) return;

        // 1. Kiểm tra xem nguyên liệu này đã nằm trong slot nào chưa, nếu có thì bỏ chọn
        var existingSlot = materialSlots.Find(s => s.SelectedWeaponUUID == materialUUID);
        if (existingSlot != null)
        {
            existingSlot.SetWeaponEmpty();
            if (materialCategory != null) materialCategory.ToggleMaterialVisibility(materialUUID, true);
            UpdateCoinText();
            return;
        }

        // 2. Nếu chưa được chọn, tìm slot trống đầu tiên để gắn vào
        var emptySlot = materialSlots.Find(s => string.IsNullOrEmpty(s.SelectedWeaponUUID));
        if (emptySlot != null)
        {
            WeaponSaveData materialData = save.Player.Inventory.GetWeapon(materialUUID);
            if (materialData != null)
            {
                ItemConfig config = gameDataBase.GetItemConfig(materialData.TemplateID);
                emptySlot.SetWeaponImage(materialUUID, config.Icon);
                if (materialCategory != null) materialCategory.ToggleMaterialVisibility(materialUUID, false);
                UpdateCoinText();
            }
        }
    }

    private void UpdateCoinText()
    {
        if (string.IsNullOrEmpty(currentTargetWeaponID) || save == null) return;
        
        WeaponSaveData data = save.Player.Inventory.GetWeapon(currentTargetWeaponID);
        if (data == null) return;
        
        int selectedCount = GetSelectedMaterials().Count;
        
        if (selectedCount == 0)
        {
            txtCoin.text = Utility.GetCoinNeedToAsscendWeapon(data.CurrentUpgrade + 1).ToString();
            return;
        }

        int totalCoin = 0;
        for (int i = 0; i < selectedCount; i++)
        {
            totalCoin += Utility.GetCoinNeedToAsscendWeapon(data.CurrentUpgrade + 1 + i);
        }
        
        txtCoin.text = totalCoin.ToString();
    }

    public void UpdateWeaponAscendCard(string weaponID)
    {
        if (weaponID != "")
        {
            currentTargetWeaponID = weaponID;

            if (materialSlots != null)
            {
                foreach (var slot in materialSlots)
                {
                    slot.SetWeaponEmpty();
                }
            }

            if (materialCategory != null)
            {
                materialCategory.Init(weaponID); // Khởi tạo và Active category khi UI bật
            }

            WeaponSaveData data = save.Player.Inventory.GetWeapon(weaponID);
            ItemConfig config = gameDataBase.GetItemConfig(data.TemplateID);
            var passiveConfig = gameDataBase.GetPassiveConfig(config.Weapon.PassiveID);

            if(data.CurrentUpgrade >= Definition.MAX_WEAPON_ASCEND)
            {
                reachedOb.gameObject.SetActive(true);
                ascendOb.gameObject.SetActive(false);
            }
            else
            {
                reachedOb.gameObject.SetActive(false);
                ascendOb.gameObject.SetActive(true);
            }

            txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
            int level = data.CurrentLevel;
            txtLevel.text = level.ToString() + "/" + Definition.MAX_WEAPON_LEVEL.ToString();

            upgradeUI.UpdateUI(data.CurrentUpgrade);

            int currentHP = config.Weapon.GetStatByLevel(StatType.HP, level);
            int currentATK = config.Weapon.GetStatByLevel(StatType.ATK, level);

            txtCurentHP.text = currentHP.ToString();
            txtCurentATK.text = currentATK.ToString();

            txtNameAndLevel.text = LocalizationManager.Instance.GetLocalizedValue(config.Name) + "(Lv." + data.CurrentUpgrade.ToString() + ")";
            txtUse.text = passiveConfig.GetDescription(data.CurrentUpgrade);
            UpdateCoinText();
        }
    }
}
