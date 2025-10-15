using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AvatarToggleUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Toggle toggle;
    private string id;

    public string ID { get { return id; } }

    public void Setup(Sprite avatarSprite, ToggleGroup group, string id)
    {
        icon.sprite = avatarSprite;
        toggle.group = group;
        this.id = id;
        toggle.onValueChanged.AddListener(OnSelected);
    }

    private void OnSelected(bool isOn)
    {
        if (isOn)
        {
            UIEvent.OnChanageAvatarPopup?.Invoke(id);
        }
    }
}
