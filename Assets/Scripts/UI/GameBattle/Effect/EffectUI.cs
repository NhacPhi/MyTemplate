using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class EffectUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _txtDuration;
    [SerializeField] private Image _tick;

    public void Setup(Sprite icon, int duration, Sprite tick)
    {
        _icon.sprite = icon;
        _txtDuration.text = duration.ToString();
        _tick.sprite = tick;
    }

    public void UpdateEffectUI(int duration)
    {
        _txtDuration.text = duration.ToString();
    }
}
