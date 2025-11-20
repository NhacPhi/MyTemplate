using UnityEngine.UI;
using TMPro;
using UnityEngine;
using VContainer;

public class CharacterCardAscend : CharacterCard
{
    [SerializeField] private TextMeshProUGUI txtName;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private Image iconRare;

    [SerializeField] private Image iconShard;
    [SerializeField] private TextMeshProUGUI txtNumberShard;
    [SerializeField] private TextMeshProUGUI txtCoin;

    [Inject] private SaveSystem save;
    [Inject] private GameDataBase gameDataBase;

    private void Awake()
    {
        UIEvent.OnSelectCharacterAvatar += UpdateCharacterCardAscend;
    }

    private void OnDestroy()
    {
        UIEvent.OnSelectCharacterAvatar -= UpdateCharacterCardAscend;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateCharacterCardAscend(save.Player.GetIDOfFirstCharacter().ID);
    }

    public void UpdateCharacterCardAscend(string id)
    {
        CharacterConfig config = gameDataBase.GetCharacterConfig(id);
        CharacterData data = save.Player.GetCharacter(id);

        txtName.text = LocalizationManager.Instance.GetLocalizedValue(config.Name);
        txtLevel.text = data.Level.ToString() + "/" + Definition.CharacterMaxLevel.ToString();
        iconRare.sprite = gameDataBase.GetCharacterRareSO(config.Rare).Icon;

        string shardID = "shard_" + config.ID;
        iconShard.sprite = gameDataBase.GetItemSOByID<ShardSO>(ItemType.Shard, shardID).Icon;
        txtNumberShard.text = save.Player.GetItem(shardID).Quantity.ToString() + "/" + Utility.GetShardNeedToUpgradeAscend(data.BoostStats + 1);
        txtCoin.text = Utility.GetCoinNeedToAscendCharacter(data.BoostStats + 1).ToString();
    }
}
