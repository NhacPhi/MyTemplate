using UnityEngine.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponConfig", menuName = "Game/Item/Weapon")]
public class WeaponSO : ItemBaseSO
{
    [SerializeField] private Sprite bigIcon;

    public Sprite BigIcon => bigIcon;
}
