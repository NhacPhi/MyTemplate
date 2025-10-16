using UnityEngine.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarIcon", menuName = "Game/Currency")]
public class CurrencySO : ScriptableObject
{
    [SerializeField] private CurrencyType type;
    [SerializeField] private string name;
    [SerializeField] private string description;
    [SerializeField] private Image icon;

    public CurrencyType Type => type;
    public string Name => name;
    public string Description => description;
    public Image Icon => icon;
}
