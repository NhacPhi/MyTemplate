using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarIcon", menuName = "Game/Item/AvatarIcon")]
public class AvatarIconSO : ScriptableObject
{
    [Tooltip("Item ID")]
    [SerializeField] private string id;

    [Tooltip("Icon")]
    [SerializeField] private Sprite icon;

    public string ID => id;

    public Sprite Icon => icon;
}
