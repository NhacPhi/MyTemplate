using UnityEngine.UI;
using UnityEngine;


[CreateAssetMenu(fileName = "Rare", menuName = "Game/ItemSaveData/CharacterSaveData/Rare")]
public class CharacterRareSO : ScriptableObject
{
    [SerializeField] private CharacterRare rare;
    [SerializeField] private Sprite icon;
    public CharacterRare Rare => rare;
    public Sprite Icon => icon;
}
