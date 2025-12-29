using UnityEngine;
using System.Collections.Generic;
using Tech.Logger;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SummonSkill : SkillRuntime, IAttackSkill, ISummonSkill, IAsyncInitializer
{
    private SummonSkillData skillData;
    private GameObject effectPrefab;
    private GameObject clonePrefab;
    private bool isLoaded = false;

    public SummonSkill(EntityStats owner, SummonSkillData data) : base(owner)
    {
        this.skillData = data;
    }

    public override void Execute(Entity caster)
    {
        //Debug.Log($"[Skill] Initialize - ID: {this.GetHashCode()}");
        _ = PerformSummon(skillData, caster); 
    }

    public override SkillData GetSkillData() => skillData;
    public void OnDealDamage(ref float damageInput)
    {
        
    }

    // Implement Summon 
    public List<Entity> ActiveSummons { get; private set; } = new List<Entity>();

    public async UniTask PerformSummon(SkillData config, Entity caster)
    {
        effectPrefab.transform.SetParent(caster.transform);
        effectPrefab.transform.localPosition = skillData.Offset;
        effectPrefab.gameObject.SetActive(true);

        await UniTask.Delay(800);

        effectPrefab.gameObject.SetActive(false);

        clonePrefab.gameObject.transform.position = caster.target.transform.position - skillData.OffsetTarget;
        clonePrefab.gameObject.SetActive(true);

        var cloneController = clonePrefab.GetComponent<CloneController>(); 

        if(cloneController != null)
        {
            await cloneController.ActiveClone();
        }

        clonePrefab.gameObject.SetActive(false);
    }                              

    public void ClearSummons()
    {
        foreach(var summon in ActiveSummons)
        {
            if (summon != null)
            {

            }
        }
    }

    //Effect
    public async void OnVFXTrigger()
    {
        
    }

    public async UniTask InitializeAsync(CancellationToken token)
    {
        var vfxRef = skillData.vfxEffectReference;
        if (vfxRef != null)
        {
            GameObject smoke = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(vfxRef);
            effectPrefab = Object.Instantiate(smoke, Vector3.zero, Quaternion.identity);
            effectPrefab.gameObject.SetActive(false);
            AddressablesManager.Instance.RemoveAsset(vfxRef);

        }

        var cloneRef = skillData.cloneReference;

        if(cloneRef != null)
        {
            GameObject clone = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(cloneRef);
            clonePrefab = Object.Instantiate(clone, Vector3.zero, Quaternion.identity);
            clonePrefab.gameObject.SetActive(false);
        }


    }
}

public class SummonSkillData : SkillData
{
    public Vector3 Offset = new Vector3(3.64f, 1.18f, 0);

    public Vector3 OffsetTarget = new Vector3(5, 0, 0);
    public string vfxEffectReference = "Smoke_Wukong";

    public string cloneReference = "Clone";
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new SummonSkill(owner, this);
}

