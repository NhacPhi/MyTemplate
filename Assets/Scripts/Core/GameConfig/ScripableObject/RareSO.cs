using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "RareBG", menuName = "Game/Item/Rare")]
public class RareSO : ScriptableObject
{
    [SerializeField] private Rare type;
    [SerializeField] private Sprite image;

    public Rare Type => type;
    public Sprite Image => image;
}
