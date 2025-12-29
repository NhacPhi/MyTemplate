using Cysharp.Threading.Tasks;
using UnityEngine;

public class CloneController : MonoBehaviour
{
    [SerializeField] private GameObject character;

    [SerializeField] private Animator smoke;

    public async UniTask ActiveClone()
    {
        smoke.SetTrigger("Start");

        await UniTask.Delay(600);

        character.gameObject.SetActive(true);

        await UniTask.Delay(1200);

        character.gameObject.SetActive(false);
    }
}
