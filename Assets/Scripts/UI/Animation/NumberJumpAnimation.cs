using DG.Tweening;
using TMPro;
using UnityEngine;

public class NumberJumpAnimation : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float normalMoveDistance = 1.0f;
    [SerializeField] private float critMoveDistance = 1.4f;
    [SerializeField] private float randomScatterX = 0.35f;

    [Header("Scale Settings")]
    [SerializeField] private float normalPeakScale = 1.25f;
    [SerializeField] private float critPeakScale = 1.6f;
    [SerializeField] private float normalEndScale = 1.0f;
    [SerializeField] private float critEndScale = 1.15f;

    [Header("Timing Settings")]
    [SerializeField] private float normalDuration = 0.65f;
    [SerializeField] private float critDuration = 0.8f;

    private CanvasGroup canvasGroup;
    private Sequence currentSequence;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void PlayAnimation(bool isCritical = false)
    {
        currentSequence?.Kill();
        transform.DOKill();
        if (canvasGroup != null) canvasGroup.DOKill();

        // 1. Reset trạng thái ban đầu
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.zero;

        // Điểm xuất phát & điểm nảy đỉnh với độ lệch X ngẫu nhiên tránh đè số
        float offsetX = Random.Range(-randomScatterX, randomScatterX);
        float moveUpDist = isCritical ? critMoveDistance : normalMoveDistance;
        Vector3 startPos = transform.position;
        Vector3 peakPos = startPos + new Vector3(offsetX, moveUpDist, 0f);

        float peakScale = isCritical ? critPeakScale : normalPeakScale;
        float endScale = isCritical ? critEndScale : normalEndScale;
        float totalDuration = isCritical ? critDuration : normalDuration;

        currentSequence = DOTween.Sequence();

        // 2. GIAI ĐOẠN 1: Bùng nổ (Pop-in cực nhanh & nảy vồng lên đỉnh)
        currentSequence.Append(transform.DOScale(peakScale, 0.12f).SetEase(Ease.OutBack));
        currentSequence.Join(transform.DOMove(peakPos, 0.22f).SetEase(Ease.OutCubic));

        // 3. GIAI ĐOẠN 2: Co về kích thước đọc & giữ nguyên nhịp cho mắt kịp đọc
        currentSequence.Insert(0.12f, transform.DOScale(endScale, 0.12f).SetEase(Ease.InOutQuad));

        if (isCritical)
        {
            // Hiệu ứng giật nhẹ tạo độ đầm/uy lực cho đòn chí mạng
            currentSequence.Insert(0.15f, transform.DOShakePosition(0.18f, strength: new Vector3(0.08f, 0.08f, 0f), vibrato: 16));
        }

        // 4. GIAI ĐOẠN 3: Trôi nhẹ lên trên và Fade Out ở 35% thời gian cuối
        float fadeStartTime = totalDuration * 0.65f;
        float fadeDuration = totalDuration - fadeStartTime;

        currentSequence.Insert(fadeStartTime, transform.DOMoveY(peakPos.y + 0.35f, fadeDuration).SetEase(Ease.InQuad));
        currentSequence.Insert(fadeStartTime, canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
    }

    public void PlayTextAnimation(float scaleMultiplier = 0.65f, float totalDuration = 0.95f)
    {
        currentSequence?.Kill();
        transform.DOKill();
        if (canvasGroup != null) canvasGroup.DOKill();

        // 1. Reset trạng thái ban đầu
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;

        Vector3 startPos = transform.position;
        Vector3 peakPos = startPos + new Vector3(0f, 0.85f, 0f);

        float peakScale = scaleMultiplier * 1.15f;
        float endScale = scaleMultiplier;

        currentSequence = DOTween.Sequence();

        // 2. Pop-in mềm mại & bay lên trên
        currentSequence.Append(canvasGroup.DOFade(1f, 0.15f));
        currentSequence.Join(transform.DOScale(peakScale, 0.18f).SetEase(Ease.OutBack));
        currentSequence.Join(transform.DOMoveY(peakPos.y, 0.35f).SetEase(Ease.OutCubic));

        // 3. Co nhẹ về kích thước chuẩn vừa mắt để người chơi đọc kịp
        currentSequence.Insert(0.18f, transform.DOScale(endScale, 0.15f).SetEase(Ease.InOutQuad));

        // 4. Giữ nguyên trên màn hình rồi trôi nhẹ lên và Fade Out
        float fadeStartTime = totalDuration * 0.6f;
        float fadeDuration = totalDuration - fadeStartTime;

        currentSequence.Insert(fadeStartTime, transform.DOMoveY(peakPos.y + 0.45f, fadeDuration).SetEase(Ease.InQuad));
        currentSequence.Insert(fadeStartTime, canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad));
    }

    private void OnDisable()
    {
        currentSequence?.Kill();
        transform.DOKill();
        if (canvasGroup != null) canvasGroup.DOKill();
    }
}
