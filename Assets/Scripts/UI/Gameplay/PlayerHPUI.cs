using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHPUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider hpSlider; // Hoặc dùng Image FillAmount
    [SerializeField] private TextMeshProUGUI hpText;

    private void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện thay đổi HP
        UIEvent.OnUpdatePlayerHP += UpdateHPUI;
    }

    private void OnDisable()
    {
        // Hủy đăng ký khi UI bị tắt để tránh lỗi bộ nhớ
        UIEvent.OnUpdatePlayerHP -= UpdateHPUI;
    }

    private void UpdateHPUI(int currentHP, int maxHP)
    {
        // Cập nhật thanh trượt HP
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        // Cập nhật chữ hiển thị (Ví dụ: 80/100)
        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
        }
    }
}
