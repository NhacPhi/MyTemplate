using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Khuyên dùng DOTween để code mượt và ngắn hơn Coroutine

public class HealthBar : BaseAttributeUI
{
    [Header("Sliders")]
    [SerializeField] private Slider healthSlider;   // Lớp máu chính (Xanh)
    [SerializeField] private Slider shieldSlider;   // Lớp giáp ảo (Trắng/Vàng)
    [SerializeField] private Slider damageSlider;   // Lớp hiệu ứng sát thương (Trắng mờ/Đỏ)

    [Header("Tick Settings")]
    [SerializeField] private GameObject tickPrefab;
    [SerializeField] private RectTransform tickContainer;
    private List<GameObject> activeTicks = new List<GameObject>();

    private const float TICK_UNIT = 2000f; // Mỗi 1000 máu 1 vạch lớn (hoặc 100 máu vạch nhỏ)
    private float maxHP;
    private float currentHP;
    private float currentShield;

    public override void Init(float hp, float maxHp)
    {
        this.maxHP = maxHp;
        this.currentHP = hp;
        this.currentShield = 0;

        UpdateVisuals(immediate: true);
    }

    public override void HandleValueChange(AttributeEvtArgs args)
    {
        switch (args.Attribute)
        {
            case AttributeType.Hp:
                float oldHP = currentHP;
                currentHP = args.Value;
                UpdateVisuals(immediate: false, isDamage: currentHP < oldHP);
                break;

            case AttributeType.Shield:
                currentShield = args.Value;
                UpdateVisuals(immediate: false);
                break;
        }
    }

    private void UpdateVisuals(bool immediate, bool isDamage = true)
    {
        // Trong LoL, nếu Shield > 0, thanh slider sẽ hiển thị dựa trên tổng (MaxHP + Shield vượt trội)
        // Tuy nhiên đơn giản nhất là giữ Slider Max = MaxHP, và Shield layer nằm đè lên.
        float totalDisplayMax = Mathf.Max(maxHP, currentHP + currentShield);

        healthSlider.maxValue = totalDisplayMax;
        shieldSlider.maxValue = totalDisplayMax;
        damageSlider.maxValue = totalDisplayMax;

        if (immediate)
        {
            healthSlider.value = currentHP;
            shieldSlider.value = currentHP + currentShield;
            damageSlider.value = currentHP;
        }
        else
        {
            // Hiệu ứng thanh máu chính
            healthSlider.value = currentHP;

            // Hiệu ứng thanh Giáp ảo (Shield nằm từ vị trí HP đi tới)
            shieldSlider.value = currentHP + currentShield;

            // Hiệu ứng thanh Damage trễ (giống LoL)
            if (isDamage)
            {
                // Đợi 0.5s rồi mới tụt thanh damageSlider
                DOTween.To(() => damageSlider.value, x => damageSlider.value = x, currentHP, 0.5f)
                    .SetDelay(0.3f)
                    .SetEase(Ease.OutQuad);
            }
            else
            {
                // Nếu là hồi máu thì thanh damage chạy theo ngay
                damageSlider.value = currentHP;
            }
        }

        UpdateTicks(totalDisplayMax);
    }

    private void UpdateTicks(float totalAmount)
    {
        int tickCount = Mathf.FloorToInt(totalAmount / TICK_UNIT);
        float containerWidth = tickContainer.rect.width;

        // Quản lý Pool đơn giản cho Ticks
        while (activeTicks.Count < tickCount)
        {
            activeTicks.Add(Instantiate(tickPrefab, tickContainer));
        }

        for (int i = 0; i < activeTicks.Count; i++)
        {
            if (i < tickCount)
            {
                activeTicks[i].SetActive(true);
                float xPos = ((i + 1) * TICK_UNIT / totalAmount) * containerWidth;
                ((RectTransform)activeTicks[i].transform).anchoredPosition = new Vector2(xPos, 0);
            }
            else
            {
                activeTicks[i].SetActive(false);
            }
        }
    }

    public override void HandleMaxValueChange(Stat stat)
    {

    }
}