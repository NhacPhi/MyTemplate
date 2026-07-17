using System.Globalization;
using TMPro;
using UnityEngine;

public class CombatTextUI : MonoBehaviour
{
    [field: SerializeField] public TextMeshProUGUI TMP { get; private set; }
    private void Reset()
    {
        LoadText();
    }

    private void Awake()
    {
        LoadText();
    }

    private void LoadText()
    {
        if (TMP) return;

        TMP = GetComponentInChildren<TextMeshProUGUI>();
    }

    [SerializeField] private GameObject iconCritical;

    public void SetValue(float damage)
    {
        string textDmg = Mathf.CeilToInt(damage).ToString(CultureInfo.InvariantCulture);
        TMP.text = textDmg;
    }

    public void SetText(string text)
    {
        TMP.text = text;
    }

    public void SetCritical(bool isCritical)
    {
        if (iconCritical != null)
        {
            iconCritical.SetActive(isCritical);
        }
    }

    public void SetAnimationEnabled(bool enabled)
    {
        // 1. Tắt/Bật script custom DOTween NumberJumpAnimation
        var jumpAnim = GetComponent<NumberJumpAnimation>();
        if (jumpAnim != null)
        {
            jumpAnim.enabled = enabled;
            if (!enabled)
            {
                // Reset lại scale và alpha của CanvasGroup khi dùng Object Pooling
                transform.localScale = Vector3.one;
                var canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }
        }

        // 2. Tắt/Bật Animator/Animation con nếu có
        var anims = GetComponentsInChildren<Animator>(true);
        foreach (var anim in anims)
        {
            anim.enabled = enabled;
            if (!enabled)
            {
                anim.transform.localScale = Vector3.one;
                anim.transform.localPosition = Vector3.zero;
            }
        }

        var animations = GetComponentsInChildren<Animation>(true);
        foreach (var anim in animations)
        {
            anim.enabled = enabled;
            if (!enabled)
            {
                anim.transform.localScale = Vector3.one;
                anim.transform.localPosition = Vector3.zero;
            }
        }

        if (!enabled && TMP != null)
        {
            Color c = TMP.color;
            c.a = 1f;
            TMP.color = c;
            
            TMP.transform.localScale = Vector3.one;
            TMP.transform.localPosition = Vector3.zero;
        }
    }
}
