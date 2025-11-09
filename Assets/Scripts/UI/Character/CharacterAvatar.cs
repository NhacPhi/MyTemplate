using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterAvatar : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Image border;

    private string id;

    public string ID => id;

    public void Init(string id,Sprite icon)
    {
        this.id = id;
        this.icon.sprite = icon;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        UIEvent.OnSelectCharacterAvatar?.Invoke(id);
    }

    public void SwitchStatus(bool value)
    {
        border.color = value ? Definition.SeletedColor : Definition.OriginColor;
        this.transform.localScale = value ? Definition.scale : Vector3.one;
    }
}
