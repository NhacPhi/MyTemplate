using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using TMPro;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private GameObject prefabAvatar;

    [SerializeField] private GameObject contentAvatar;

    [SerializeField] private ScrollRect scrollRectl;

    [SerializeField] private List<GameObject> cards;


    List<GameObject> avatars = new();

    [Inject] private IObjectResolver _objectResolver;
    [Inject] private GameDataBase gameDataBase;
    [Inject] private SaveSystem save;
    [Inject] private CurrencyManager currencyMM;
    [Inject] private CharacterStatManager characterStatMM;

    [SerializeField] private Image characterImage;
    [SerializeField] private CharacterWeaponUI waeponUI;
    [SerializeField] private TextMeshProUGUI txtPower;

    [SerializeField] private GameObject characterStatInfo;
    [SerializeField] private GameObject characterWeapon;

    [SerializeField] private List<CharacterToggle> taps;

    private string currentCharacter = "";
    private CharacterTap currentTap = CharacterTap.None;

    private bool isSelectedRelicTap = false;
    private bool isOpenWeaponCategpry = false;
    private void OnEnable()
    {
        UIEvent.OnSelectCharacterAvatar += SelectCharacterAvatar;
        UIEvent.OnSelectToggleCharacterTap += ShowCharacterCard;
        UIEvent.OnSelectCharacterChangeWeapon += ShowCharacterWeapon;
        UIEvent.OnCloseCharacterWeapon += CloseCharacterWeapon;
        UIEvent.OnSlectectRelicTap += OnSelectedRelicTap;
        if (avatars.Count > 0 )
        {
            ResetUI();
            SelectCharacterAvatar(avatars[0].gameObject.GetComponent<CharacterAvatar>().ID);
            ShowCharacterCard(CharacterTap.Info);
        }
        scrollRectl.normalizedPosition = new Vector2(0, 1);
    }

    private void OnDisable()
    {
        UIEvent.OnSelectCharacterAvatar -= SelectCharacterAvatar;
        UIEvent.OnSelectToggleCharacterTap -= ShowCharacterCard;

        UIEvent.OnSelectCharacterChangeWeapon -= ShowCharacterWeapon;
        UIEvent.OnCloseCharacterWeapon -= CloseCharacterWeapon;
        UIEvent.OnSlectectRelicTap -= OnSelectedRelicTap;
    }
    private void Start()
    {

    }
    public void Init()
    {
        _objectResolver.Inject(this);
        foreach (var character in save.Player.Characters)
        {
            GameObject obj = Instantiate(prefabAvatar, contentAvatar.transform);
            string weaponID = save.Player.GetCharacter(character.ID).Weapon;
            obj.GetComponent<CharacterAvatar>().Init(character.ID, weaponID, gameDataBase.GetItemSOByID<ShardSO>(ItemType.Shard,"shard_" + character.ID).Icon);
            avatars.Add(obj);
        }

        currencyMM.UpdateCurrency();
        ResetUI();
        ShowCharacterCard(CharacterTap.Info);
    }

    private void ResetUI()
    {
        currentCharacter = save.Player.Characters[0].ID;
        avatars[0].gameObject.GetComponent<CharacterAvatar>().SwitchStatus(true);
        ClickOnFristIconAvatar();
        characterImage.sprite = gameDataBase.GetCharacterSO(save.Player.Characters[0].ID).Icon;
        currentTap = CharacterTap.None;
        UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
    }

    void ClickOnFristIconAvatar()
    {
        avatars[0].gameObject.GetComponent<CharacterAvatar>().HandleOnClickEvent();
        CloseCharacterWeapon(true);
    }

    public void SelectCharacterAvatar(string id)
    {
        currentCharacter = id;
        characterImage.sprite = gameDataBase.GetCharacterSO(id).Icon;
        string weaponID = save.Player.GetCharacter(currentCharacter).Weapon;
        if(weaponID != "")
        {
            waeponUI.SetWeaponImage(gameDataBase.GetItemSOByID<WeaponSO>(ItemType.Weapon, weaponID).BigIcon);
        }
        else
        {
            waeponUI.SetWeaponEmpty();
        }

        foreach(var obj in avatars)
        {
            CharacterAvatar avatar = obj.gameObject.GetComponent<CharacterAvatar>();
            if(avatar != null)
            {
                if(avatar.ID == id)
                {
                    avatar.SwitchStatus(true);
                }else
                {
                    avatar.SwitchStatus(false);
                }
            }
        }
        txtPower.text = characterStatMM.GetCharacterPower(id).ToString();

        foreach (var obj in avatars)
        {
            CharacterAvatar avatar = obj.gameObject.GetComponent<CharacterAvatar>();
            if (isOpenWeaponCategpry)
            {
                avatar.IsShowWeaponCategory = true;
            }
            else
            {
                avatar.IsShowWeaponCategory = false;
            }
        }
    }

    public void ShowCharacterCard(CharacterTap type)
    {
        if (type == currentTap) return;

        foreach(var obj in cards)
        {
            var card = obj.gameObject.GetComponent<CharacterCard>();
            if(card != null)
            {
                if(card.Type == type)
                {
                    currentTap = card.Type;
                    card.gameObject.SetActive(true);
                }
                else
                    card.gameObject.SetActive(false);
            }
        }

        foreach(var tap in taps)
        {
            if (tap.Type == type)
            {
                tap.ActiveToggle();
            }
        }
    }

    public void ShowCharacterWeapon(string id)
    {
        if(isSelectedRelicTap)
        {
            characterStatInfo.gameObject.SetActive(false);
            characterWeapon.gameObject.SetActive(true);
            isOpenWeaponCategpry = true;
        }
    }

    public void CloseCharacterWeapon(bool close)
    {
        if(close)
        {
            characterStatInfo.gameObject.SetActive(true);
            characterWeapon.gameObject.SetActive(false);
            isOpenWeaponCategpry = false;
        }
    }

    public void OnSelectedRelicTap(bool value)
    {
        isSelectedRelicTap = value;
    }
}
