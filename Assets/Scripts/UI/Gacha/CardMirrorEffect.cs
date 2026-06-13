using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class CardMirrorEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("Tilt Settings")]
    [SerializeField] private float tiltAmount = 15f;
    [SerializeField] private float tiltSpeed = 0.2f;

    [Header("Glare Effect")]
    [SerializeField] private bool enableGlare = true;
    [SerializeField] private float glareDuration = 0.6f;
    
    private RectTransform rectTransform;
    private Vector2 startRotation;
    
    // Glare
    private RectTransform glareRect;
    private Image glareImage;
    private bool isHovering = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startRotation = rectTransform.localEulerAngles;

        if (enableGlare)
        {
            CreateGlareEffect();
        }
    }

    private void CreateGlareEffect()
    {
        // Tìm xem thẻ bài có Mask ở đâu không (thường mask nằm ở khung viền hoặc hình nền)
        Transform targetParent = this.transform;
        
        var mask = GetComponentInChildren<UnityEngine.UI.Mask>();
        if (mask != null) 
        {
            targetParent = mask.transform;
        }
        else 
        {
            var rectMask = GetComponentInChildren<UnityEngine.UI.RectMask2D>();
            if (rectMask != null) 
            {
                targetParent = rectMask.transform;
            }
        }

        // Tạo một tia sáng vắt ngang thẻ bài
        GameObject glareObj = new GameObject("CardGlare");
        glareRect = glareObj.AddComponent<RectTransform>();
        glareRect.SetParent(targetParent, false); // false để giữ tỷ lệ local
        glareRect.localScale = Vector3.one;
        glareRect.localRotation = Quaternion.Euler(0, 0, 30f); // Xoay xéo vệt sáng

        // Trải dài vượt quá thẻ để lướt qua không bị cắt
        glareRect.anchorMin = new Vector2(0.5f, 0.5f);
        glareRect.anchorMax = new Vector2(0.5f, 0.5f);
        glareRect.pivot = new Vector2(0.5f, 0.5f);
        glareRect.sizeDelta = new Vector2(100f, 1500f); 

        glareImage = glareObj.AddComponent<Image>();
        glareImage.color = new Color(1f, 1f, 1f, 0f); // Ẩn lúc ban đầu
        glareImage.raycastTarget = false;

        // Đặt ở một bên của thẻ bài
        glareRect.anchoredPosition = new Vector2(-1000f, 0);
    }

    public void PlayGlareSweep()
    {
        if (glareRect == null || glareImage == null) return;

        // Reset vị trí và hiện sáng
        glareRect.DOKill();
        glareImage.DOKill();

        glareRect.anchoredPosition = new Vector2(-1000f, 0);
        glareImage.color = new Color(1f, 1f, 1f, 0.5f);

        // Chạy qua phải
        glareRect.DOAnchorPosX(1000f, glareDuration).SetEase(Ease.InOutSine).SetLink(glareRect.gameObject);
        // Fade mờ dần ở cuối
        glareImage.DOFade(0f, glareDuration).SetEase(Ease.InExpo).SetLink(glareImage.gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        // Bắn tia sáng lướt qua khi vừa đưa chuột vào
        PlayGlareSweep();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        // Trả về thẳng đứng mượt mà
        rectTransform.DORotate(startRotation, tiltSpeed).SetEase(Ease.OutBack).SetLink(rectTransform.gameObject);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!isHovering) return;

        // Tính toán vị trí chuột tương đối trên lá bài (-1 đến 1)
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPoint);
        
        Vector2 normalizedPos = new Vector2(
            (localPoint.x / rectTransform.rect.width) * 2f,
            (localPoint.y / rectTransform.rect.height) * 2f
        );

        // Clamp để giới hạn
        normalizedPos.x = Mathf.Clamp(normalizedPos.x, -1f, 1f);
        normalizedPos.y = Mathf.Clamp(normalizedPos.y, -1f, 1f);

        // Tính góc xoay ngược lại với hướng chuột để tạo hiệu ứng nổi
        float rotateX = -normalizedPos.y * tiltAmount;
        float rotateY = normalizedPos.x * tiltAmount;

        rectTransform.DOKill();
        rectTransform.DOLocalRotate(new Vector3(rotateX, rotateY, 0), tiltSpeed).SetLink(rectTransform.gameObject);
    }
}
