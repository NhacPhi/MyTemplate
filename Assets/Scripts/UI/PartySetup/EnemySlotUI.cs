using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class EnemySlotUI : MonoBehaviour
{
    [SerializeField] private Image _imageCharacter;
    [SerializeField] private TextMeshProUGUI _txtName;

    Vector2 fixedSize;
    private void Awake()
    {
        fixedSize = _imageCharacter.rectTransform.sizeDelta;
    }

    public void SetupEnemySlotUI(Sprite sprite, string name)
    {
        _imageCharacter.sprite = sprite;
        _txtName.text = name;
        _imageCharacter.rectTransform.sizeDelta = fixedSize;
    }
}
