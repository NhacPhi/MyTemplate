using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterUIResult : GameItemUI
{
    [Header("Character Info")]
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private GameObject upgradesParent; // Parent object chứa các UpgradeUI

    [Header("Battle Info")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private GameObject deadOverlay; // GameObject (như Image đen thui, icon sọ người, chữ ngỏm...) hiển thị khi chết

    private UpgradeUI[] upgrades;

    private void Awake()
    {
        if (upgradesParent != null)
        {
            upgrades = upgradesParent.GetComponentsInChildren<UpgradeUI>();
        }
    }

    public void Init(string id, Rare rare, Sprite icon, Sprite background, int level, int upgradeNumber, int currentHp, int maxHp)
    {
        // 1. Kế thừa việc Setup UI gốc (Khung nền, độ hiếm, avatar...)
        base.Setup(id, rare, icon, background);
        
        // 2. Hiển thị Level
        if (txtLevel != null)
        {
            txtLevel.text = level.ToString();
        }

        // 3. Hiển thị số lượng sao/bậc nâng cấp giống với CharacterIconUI
        if (upgrades != null)
        {
            for (int i = 0; i < upgrades.Length; i++)
            {
                if (i < upgradeNumber)
                {
                    upgrades[i].ActiveLayer(1);
                }
                else
                {
                    upgrades[i].ActiveLayer(0);
                }
            }
        }

        // 4. Cập nhật thanh HP
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
            Debug.Log($"[CharacterUIResult] {id} HP Update: {currentHp} / {maxHp}");
        }
        else
        {
            Debug.LogError($"[CharacterUIResult] {id} hpSlider is NULL! Please assign it in the prefab.");
        }

        // 5. Trạng thái Sống / Chết
        bool isDead = (currentHp <= 0);
        if (deadOverlay != null)
        {
            deadOverlay.SetActive(isDead);
        }
    }
}
