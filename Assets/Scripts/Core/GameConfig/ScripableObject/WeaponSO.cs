using UnityEngine.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponConfig", menuName = "Game/Item/Weapon")]
public class WeaponSO : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite bigIcon;

    public string ID => id;
    public Sprite Icon => icon;
    public Sprite BigIcon => bigIcon;
}
