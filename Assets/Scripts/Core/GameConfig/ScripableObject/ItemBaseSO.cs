using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FoodSO", menuName = "Game/ItemData/Item")]
public class ItemBaseSO : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private Sprite icon;

    public string ID => id;

    public Sprite Icon => icon;
}
