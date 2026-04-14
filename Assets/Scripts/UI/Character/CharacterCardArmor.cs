using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;
using UnityEngine.UI;

public class CharacterCardArmor : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtATK;
    [SerializeField] private TextMeshProUGUI txtDEF;
    [SerializeField] private TextMeshProUGUI txtSPD;
    [SerializeField] private TextMeshProUGUI txtDEFShred;
    [SerializeField] private TextMeshProUGUI txtCritRate;
    [SerializeField] private TextMeshProUGUI txtCritDMG;
    [SerializeField] private TextMeshProUGUI txtPenetration;
    [SerializeField] private TextMeshProUGUI txtCritDGMRes;

    [SerializeField] private List<CharacterArmorUI> armors;

    [SerializeField] private Button btnQuickUnequip;
    [SerializeField] private Button btnQuickEquip;

    [SerializeField] private List<SetBonusUI> setBonusUIs; 

    [Inject] private PlayerCharacterManager playerCharacterManager;
    [Inject] private InventoryManager inventory;
    [Inject] private GameDataBase gameDataBase;

    List<ArmorSaveData> ArmorSaveDatas = new();
    private string currentCharacterID = "";

    private string armorPartOfCharacter = "";
    private string currentArmorPartSelected = "";
    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardArmor;
        UIEvent.OnSelectCharacterArmorUI += SelectCharacterArmorUI;

        if (btnQuickUnequip != null)
        {
            btnQuickUnequip.onClick.AddListener(OnBtnQuickUnequipClicked);
        }

        if(btnQuickEquip != null)
        {
            btnQuickEquip.onClick.AddListener(OnBtnQuickEquipClicked);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateCharacterCardArmor(playerCharacterManager.GetFirstCharacter().SaveData.ID);
    }

    private void OnEnable()
    {
        ResetUI();
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardArmor;
        UIEvent.OnSelectCharacterArmorUI -= SelectCharacterArmorUI;

        btnQuickEquip.onClick.RemoveAllListeners();
        btnQuickUnequip.onClick.RemoveAllListeners();
    }

    public void UpdateCharacterCardArmor(string id)
    {
        currentCharacterID = id;
        float hp, atk, def, spd, defShred, critRate, critDMG, penetration, critDMGRes;
        hp = atk = def = spd = defShred = critRate = critDMG = penetration = critDMGRes = 0;

        var characterProfile = playerCharacterManager.GetCharacter(id);
        var characterSaveData = characterProfile.SaveData;

        ArmorSaveDatas.Clear();
        if (characterSaveData.Armors.Count > 0)
        {

            foreach(var ArmorSaveData in characterSaveData.Armors)
            {
                ArmorSaveDatas.Add(inventory.GetArmor(ArmorSaveData.ID));
            }

            //BaseArmorConfig armor = gameDataBase.GetItemConfigByID<BaseArmorConfig>(ItemType.Armor, id);

            foreach (var armor in armors)
            {
                // check armor patt exit in Character
                if(CheckArmorPartExit(armor.Type, id))
                {
                    armor.SwitchStatusArmorUI(false);

                    PartSaveData part = characterSaveData.Armors.Find(part => part.Type == armor.Type);
                    ArmorSaveData armorData = ArmorSaveDatas.Find(armor => armor.UUID == part.ID);
                    ItemConfig armorConfig = gameDataBase.GetItemConfig(armorData.TemplateID);

                    armor.UpdateArmorUI(armorData.UUID, armorData.Rare, armorConfig.Icon, gameDataBase.GetBGItemByRare(armorData.Rare), armorData.Level);
                }
                else
                {
                    armor.ResetUI();
                    armor.SwitchStatusArmorUI(true);
                }
            }

            txtHP.text = characterProfile.GetTotalArmorConstantStat(StatType.HP).ToString();
            txtATK.text = characterProfile.GetTotalArmorConstantStat(StatType.ATK).ToString();
            txtDEF.text = characterProfile.GetTotalArmorConstantStat(StatType.DEF).ToString();
            txtSPD.text = characterProfile.GetTotalArmorConstantStat(StatType.SPEED).ToString();
            txtDEFShred.text = defShred.ToString();
            txtCritRate.text = critRate.ToString();
            txtCritDMG.text = critDMG.ToString();
            txtPenetration.text = penetration.ToString();
            txtCritDGMRes.text = critDMGRes.ToString();
        }
        else
        {
            foreach (var armor in armors)
            {
                armor.ResetUI();
                armor.SwitchStatusArmorUI(true);
            }

            txtHP.text = hp.ToString();
            txtATK.text = atk.ToString();
            txtDEF.text = def.ToString();
            txtSPD.text = spd.ToString();
            txtDEFShred.text = defShred.ToString();
            txtCritRate.text = critRate.ToString();
            txtCritDMG.text = critDMG.ToString();
            txtPenetration.text = penetration.ToString();
            txtCritDGMRes.text = critDMGRes.ToString();
        }

        foreach(var ui in setBonusUIs)
        {
            ui.gameObject.SetActive(false);
        }
        var setBonusConfigs = characterProfile.GetSetBonusActive();
        if(setBonusConfigs!= null )
        {
            for(int i = 0; i < setBonusConfigs.Count; i++)
            {
                var name = LocalizationManager.Instance.GetLocalizedValue(setBonusConfigs[i].Name);
                var content = setBonusConfigs[i].GetConentBonus();
                setBonusUIs[i].UpdateSetBonusUI(name, content);

                setBonusUIs[i].gameObject.SetActive(true);
            }
        }
    }


    private bool CheckArmorPartExit(ArmorPart part, string id)
    {
        return (playerCharacterManager.GetCharacter(id).SaveData.Armors.Any(armor => armor.Type == part));
    }

    private void SelectCharacterArmorUI(string id)
    {
        armorPartOfCharacter = id;
        
        if (id != "")
        {
            PartSaveData part = playerCharacterManager.GetCharacter(currentCharacterID).SaveData.Armors.Find(part => part.ID == id);

            for (int i = 0; i < armors.Count; i++)
            {
                if (armors[i].Type == part.Type)
                {
                    armors[i].OnSwitchStatusBoder(true);
                }
                else
                {
                    armors[i].OnSwitchStatusBoder(false);
                }
            }
        }
    }

    private void ResetUI()
    {
        foreach(var armorUI in armors)
        {
            armorUI.OnSwitchStatusBoder(false);
        }
    }

    private void OnBtnQuickUnequipClicked()
    {
        if (string.IsNullOrEmpty(currentCharacterID)) return;

        var character = playerCharacterManager.GetCharacter(currentCharacterID);
        if (character != null)
        {
            character.UnequipAllArmors();
            UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacterID);
            ResetUI();
        }
    }

    private void OnBtnQuickEquipClicked()
    {
        if (string.IsNullOrEmpty(currentCharacterID)) return;

        var character = playerCharacterManager.GetCharacter(currentCharacterID);
        if (character != null)
        {
            character.QuickEquipArmor();
            UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacterID);
            ResetUI();
        }
    }
}
