using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

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
    [SerializeField] private Image characterImage;

    private string currentCharacter = "";
    private CharacterTap currentTap = CharacterTap.None;

    private void OnEnable()
    {
        UIEvent.OnSelectCharacterAvatar += SelectCharacterAvatar;
        UIEvent.OnSelectToggleCharacterTap += ShowCharacterCard;
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
    }

    public void Init()
    {
        _objectResolver.Inject(this);
        foreach (var character in save.Player.Characters)
        {
            GameObject obj = Instantiate(prefabAvatar, contentAvatar.transform);
            obj.GetComponent<CharacterAvatar>().Init(character.ID, gameDataBase.GetItemSOByID<ShardSO>(ItemType.Shard,"shard_" + character.ID).Icon);
            avatars.Add(obj);
        }
        ResetUI();
        ShowCharacterCard(CharacterTap.Info);
    }

    private void ResetUI()
    {
        currentCharacter = save.Player.Characters[0].ID;
        avatars[0].gameObject.GetComponent<CharacterAvatar>().SwitchStatus(true);
        characterImage.sprite = gameDataBase.GetCharacterSO(save.Player.Characters[0].ID).Icon;
        currentTap = CharacterTap.None;
        UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
    }

    public void SelectCharacterAvatar(string id)
    {
        currentCharacter = id;
        characterImage.sprite = gameDataBase.GetCharacterSO(id).Icon;
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
    }
}
