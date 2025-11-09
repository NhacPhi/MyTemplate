using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIFramework;
using UnityEngine.UI;
using VContainer;

public class PopupChangeAvatarController : WindowController
{
    [SerializeField] private Button btnComfirm;
    [SerializeField] private Button btnCancel;

    [SerializeField] private GameObject avatarPrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private ToggleGroup toggleGroup;

    [SerializeField] private Image avatarIcon;

    [Inject] private GameDataBase gameDataBase;

    [Inject] private SaveSystem save;

    private List<Toggle> toggles = new List<Toggle>();

    private string currentAvatarID;
    private void Start()
    {
        btnCancel.onClick.AddListener(OnCancel);
        btnComfirm.onClick.AddListener(OnConfirm);

        currentAvatarID = save.Player.AvatarIcon;

        GenerateAvatars();

        UpdateAvatarIcon(currentAvatarID);
    }

    private void OnEnable()
    {
        UIEvent.OnChanageAvatarPopup += UpdateAvatarIcon;
    }

    private void OnDisable()
    {
        UIEvent.OnChanageAvatarPopup -= UpdateAvatarIcon;
    }
    public void OnConfirm()
    {
        base.UI_Close();

        UIEvent.OnChanageAvatarPanel?.Invoke(currentAvatarID);

        save.Player.AvatarIcon = currentAvatarID;
    }

    public void OnCancel()
    {
        base.UI_Close();
    }

    void GenerateAvatars()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (var avatar in gameDataBase.Avatars)
        {
            var obj = Instantiate(avatarPrefab, gridParent);
            var avatarUI = obj.GetComponent<AvatarToggleUI>();
            avatarUI.Setup(avatar.Icon, toggleGroup, avatar.ID);
            var togle = obj.GetComponent<Toggle>();
            if (togle != null)
            {
                toggles.Add(togle);
            }
        }

        foreach (var t in toggles)
        {
            if (t.gameObject.GetComponent<AvatarToggleUI>().ID.Equals(currentAvatarID))
            {
                t.isOn = true;
                break;
            }
        }
    }

    public void UpdateAvatarIcon(string id)
    {
        avatarIcon.sprite = gameDataBase.GetItemSOByID<AvatarIconSO>(ItemType.Avatar, id).Icon;
        currentAvatarID = id;
    }
}
