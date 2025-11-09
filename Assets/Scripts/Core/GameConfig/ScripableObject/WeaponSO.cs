using UnityEngine.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponConfig", menuName = "Game/ItemData/WeaponData")]
public class WeaponSO : ItemBaseSO
{
    [SerializeField] private Sprite bigIcon;

    public Sprite BigIcon => bigIcon;
}
