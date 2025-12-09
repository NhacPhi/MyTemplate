using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberJumpAnimation : MonoBehaviour
{
    public float MoveUpDistance = 1f;
    public float Duration = 0.6f;
    public float StartScale = 0.5f;
    public float PeakScale = 1.2f;
    public float EndScale = 1f;

    private TextMeshProUGUI text;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        PlayAnimation();
    }

    public void PlayAnimation()
    {
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one * StartScale;

        Sequence s = DOTween.Sequence();

        s.Append(
            transform.DOMoveY(transform.position.y + MoveUpDistance, Duration)
                .SetEase(Ease.OutQuad)
        );

        s.Join(
            canvasGroup.DOFade(0, Duration)
        );


        s.Insert(0,
            transform.DOScale(PeakScale, Duration * 0.3f)
                .SetEase(Ease.OutBack)
        );

        s.Insert(Duration * 0.3f,
            transform.DOScale(EndScale, Duration * 0.7f)
        );

    }
}
