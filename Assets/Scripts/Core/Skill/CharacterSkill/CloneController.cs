using Cysharp.Threading.Tasks;
using UnityEngine;

public class CloneController : MonoBehaviour
{
    [SerializeField] private GameObject character;

    [SerializeField] private Animator smoke;

    public async UniTask ActiveClone(DamageBonus damageBonus, Tech.Composite.Core source, Tech.Composite.Core target)
    {
        smoke.SetTrigger("Start");

        await UniTask.Delay(600);

        character.gameObject.SetActive(true);

        await UniTask.Delay(600);

        DamageFormular.DealDamage(damageBonus, source, target);

        await UniTask.Delay(600);

        character.gameObject.SetActive(false);
    }
}
