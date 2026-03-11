using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CharacterSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _imageCharacter;
    [SerializeField] private TextMeshProUGUI _txtName;
    
    private CanvasGroup _canvasGroup;
    private PartySetupControllerUI _controller;
    public int CurrentPosition;
    private Vector3 _startPosition;

    private string characterID;
    public string CharacterID => characterID;
    Vector2 fixedSize;
    private void Awake()
    {
        fixedSize = _imageCharacter.rectTransform.sizeDelta;
    }

    public void Init(int pos, PartySetupControllerUI controller, string id)
    {
        CurrentPosition = pos;
        _controller = controller;
        _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        characterID = id;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPosition = transform.position;
        _controller.SetAllCharactersRaycast(false);
        _canvasGroup.alpha = 0.7f;
        transform.SetAsLastSibling(); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _controller.SetAllCharactersRaycast(true);
        _canvasGroup.alpha = 1f;

        // Kiểm tra xem chuột có đang nằm trên một Slot nào không
        GameObject hoveredObj = eventData.pointerCurrentRaycast.gameObject;

        if (hoveredObj != null)
        {
            // Lúc này hoveredObj chắc chắn sẽ là PartySlot vì các Character khác đã bị "vô hình" với Raycast
            PartySlot targetSlot = hoveredObj.GetComponent<PartySlot>();

            if (targetSlot != null && targetSlot.PositionIndex != this.CurrentPosition)
            {
                _controller.SwapActiveSlots(this.CurrentPosition, targetSlot.PositionIndex);
                return;
            }
        }

        // Nếu thả ra ngoài hoặc trùng vị trí cũ, quay về chỗ cũ
        transform.DOMove(_startPosition, 0.2f);
    }

    public void SetupCharacterSlotUI(Sprite sprite, string name)
    {
        _imageCharacter.sprite = sprite;
        _txtName.text = name;
        _imageCharacter.rectTransform.sizeDelta = fixedSize;
    }
}
