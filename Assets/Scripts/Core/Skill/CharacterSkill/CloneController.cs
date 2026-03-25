using Cysharp.Threading.Tasks;
using UnityEngine;

public class CloneController : MonoBehaviour
{
    [SerializeField] private GameObject character;

    [SerializeField] private Animator smoke;

    public async UniTask ActiveClone(DamageBonus damageBonus, Tech.Composite.Core caster, Tech.Composite.Core target)
    {
        smoke.SetTrigger("Start");

        caster.GetComponent<Entity>().PlaySFX("Summon_Smoke");

        await UniTask.Delay(600);

        character.gameObject.SetActive(true);
        caster.GetComponent<Entity>().PlaySFX("SunWukong_Attack");
        await UniTask.Delay(600);

        DamageFormular.DealDamage(damageBonus, caster, target);

        await UniTask.Delay(600);

        character.gameObject.SetActive(false);
    }
}
