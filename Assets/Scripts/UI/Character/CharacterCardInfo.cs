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
    [Inject] private IObjectResolver _objectResolver;
    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;

    private void OnEnable()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardInfo;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardInfo;
    }

    // Start is called before the first frame update
    void Start()
    {
        _objectResolver.Inject(this);
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


        txtHP.text = "1";
        txtATK.text = "2";
        txtDEF.text = "3";
        txtSPD.text = "9";
        txtDEFShred.text = "4";
        txtCritRate.text = "5";
        txtCriteDMG.text = "6";
        txtPenetration.text = "7";
        txtCritDGMRes.text = "8";
    }
}
