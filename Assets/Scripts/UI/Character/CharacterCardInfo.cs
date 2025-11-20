using UnityEngine.UI;
using UnityEngine;
using TMPro;
using VContainer;

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
        CharacterSO so = gameDataBase.GetCharacterSO(id);
        CharacterConfig config = gameDataBase.GetCharacterConfig(id);
        CharacterData data = save.Player.GetCharacter(id);
        if(so == null || config == null || data == null)
        {
            Debug.Log("Character Data null with id: " + id);
            return;
        }
        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = data.Level.ToString() + "/" + Definition.CharacterMaxLevel.ToString() ;
        iconRare.sprite = gameDataBase.GetCharacterRareSO(config.Rare).Icon;

        CharacterStatConfig stat = characterStatMM.GetCharacterStat(id);

        txtHP.text = stat.HP.ToString();
        txtATK.text = stat.ATK.ToString();
        txtDEF.text = stat.DEF.ToString();
        txtSPD.text = stat.SPD.ToString();
        txtDEFShred.text = stat.DEFShred.ToString();
        txtCritRate.text = stat.CRITRate.ToString();
        txtCriteDMG.text = stat.CRITDMG.ToString();
        txtPenetration.text = stat.Penetration.ToString();
        txtCritDGMRes.text = stat.CRITDMGRes.ToString();
    }
}
