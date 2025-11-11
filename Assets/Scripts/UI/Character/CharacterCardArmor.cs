using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;

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
    [Inject] private IObjectResolver _objectResolver;
    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;

    List<ArmorData> armorDatas = new();
    private string currentCharacterID = "";
    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardArmor;
        UIEvent.OnSelectCharacterArmorUI += SelectCharacterArmorUI;
    }
    // Start is called before the first frame update
    void Start()
    {
        _objectResolver.Inject(this);
    }

    private void OnEnable()
    {
        ResetUI();
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardArmor;
        UIEvent.OnSelectCharacterArmorUI -= SelectCharacterArmorUI;
    }

    public void UpdateCharacterCardArmor(string id)
    {
        currentCharacterID = id;
        float hp, atk, def, spd, defShred, critRate, critDMG, penetration, critDMGRes;
        hp = atk = def = spd = defShred = critRate = critDMG = penetration = critDMGRes = 0;

        if(save.Player.Armors.Count > 0)
        {
            armorDatas.Clear();
            CharacterData data = save.Player.GetCharacter(id);

            foreach(var armorData in data.Armors)
            {
                armorDatas.Add(save.Player.GetArmor(armorData.ID));
            }

            //BaseArmorConfig armor = gameDataBase.GetItemConfigByID<BaseArmorConfig>(ItemType.Armor, id);

            foreach (var armor in armors)
            {
                // check armor patt exit in Character
                if(CheckArmorPartExit(armor.Type, id))
                {
                    armor.SwitchStatusArmorUI(false);

                    Part part = save.Player.GetCharacter(id).Armors.Find(part => part.Type == armor.Type);
                    ArmorData armordata = armorDatas.Find(armor => armor.InstanceID == part.ID);
                    BaseArmorConfig config = gameDataBase.GetItemConfigByID<BaseArmorConfig>(ItemType.Armor, armordata.TemplateID);
                    ArmorSO armorSO = gameDataBase.GetItemSOByID<ArmorSO>(ItemType.Armor, armordata.TemplateID);

                    armor.UpdateArmorUI(armordata.InstanceID, armordata.Rare, armorSO.Icon, gameDataBase.GetRareBG(armordata.Rare), armordata.Level);
                }
                else
                {
                    armor.SwitchStatusArmorUI(true);
                }
            }
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


    private bool CheckArmorPartExit(ArmorPart part, string id)
    {
        return (save.Player.GetCharacter(id).Armors.Any(armor => armor.Type == part));
    }

    private void SelectCharacterArmorUI(string id)
    {
        Part part = save.Player.GetCharacter(currentCharacterID).Armors.Find(part => part.ID == id);
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

    private void ResetUI()
    {
        foreach(var armorUI in armors)
        {
            armorUI.OnSwitchStatusBoder(false);
        }
    }
}
