using UnityEngine;


[CreateAssetMenu(fileName = "newActor", menuName = "Game/Narrative/Actor")]
public class ActorSO : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private Sprite texture;

    public string ID => id;
    public Sprite Texture => texture;
}
