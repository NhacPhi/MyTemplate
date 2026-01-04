using UnityEngine.UI;
using UnityEngine;
using TMPro;
using VContainer;
using System.Collections.Generic;

public class CharacterCardInfo : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Image iconRare;

    [SerializeField] private TextMeshProUGUI txtHP;
    [SerializeField] private TextMeshProUGUI txtATK;
    [SerializeField] private TextMeshProUGUI txtDEF;
    [SerializeField] private TextMeshProUGUI txtSPD;
    [SerializeField] private TextMeshProUGUI txtDEFShred;
    [SerializeField] private TextMeshProUGUI txtCritRate;
    [SerializeField] private TextMeshProUGUI txtCriteDMG;
    [SerializeField] private TextMeshProUGUI txtPenetration;
    [SerializeField] private TextMeshProUGUI txtCritDGMRes;

    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;

    [Inject] private CharacterStatManager characterStatMM;

    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardInfo;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateCharacterCardInfo(save.Player.GetIDOfFirstCharacter().ID);
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardInfo;
    }
    public void UpdateCharacterCardInfo(string id)
    {
        CharacterConfig config = gameDataBase.GetCharacterConfig(id);
        CharacterSaveData data = save.Player.GetCharacter(id);

        if(config == null || data == null)
        {
            Debug.Log("Character Data null with id: " + id);
            return;
        }
        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = data.Level.ToString() + "/" + Definition.CharacterMaxLevel.ToString() ;
        //iconRare.sprite = gameDataBase.GetCharacterRareSO(config.Rare).Icon;

        txtHP.text = config.Stats.GetValueOrDefault(StatType.HP).ToString();
        txtATK.text = config.Stats.GetValueOrDefault(StatType.ATK).ToString();
        txtDEF.text = config.Stats.GetValueOrDefault(StatType.DEF).ToString(); // stat.DEF.ToString();
        txtSPD.text = "0"; // stat.SPD.ToString();
        txtDEFShred.text = "0"; // stat.DEFShred.ToString();
        txtCritRate.text = "0"; // stat.CRITRate.ToString();
        txtCriteDMG.text = "0"; // stat.CRITDMG.ToString();
        txtPenetration.text = "0"; // stat.Penetration.ToString();
        txtCritDGMRes.text = "0"; // stat.CRITDMGRes.ToString();
    }
}
