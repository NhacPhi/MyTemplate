using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering;

public class SummonSkill : SkillRuntime, IAttackSkill, ISummonSkill, IAsyncInitializer
{
    private SummonSkillData skillData;
    private GameObject effectPrefab;
    private List<GameObject> clonePrefabs = new List<GameObject>();


    public SummonSkill(EntityStats owner, SummonSkillData data) : base(owner)
    {
        this.skillData = data;
    }

    public override async UniTask ExecuteAsync(Entity caster)
    {
        //Debug.Log($"[SkillCharacter] Initialize - ID: {this.GetHashCode()}");
        await PerformSummon(skillData, caster); 
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

        for(int i = 0; i < caster.Targets.Count; i++)
        {
            clonePrefabs[i].gameObject.transform.position = caster.Targets[i].transform.position - skillData.OffsetTarget;

            clonePrefabs[i].gameObject.GetComponent<SortingGroup>().sortingOrder = caster.Targets[i].GetComponent<SortingGroup>().sortingOrder + 1;

            clonePrefabs[i].gameObject.SetActive(true);
        }
        List<UniTask> activeCloneTasks = new List<UniTask>();

        for (int i = 0; i < caster.Targets.Count; i++)
        {
            var cloneController = clonePrefabs[i].GetComponent<CloneController>();

            if (cloneController != null)
            {
                activeCloneTasks.Add(cloneController.ActiveClone(CalculateRawDamage(), caster, caster.Targets[i].GetComponent<Entity>()));
                int randomStartDelay = UnityEngine.Random.Range(100, 300);
                await UniTask.Delay(randomStartDelay);
            }
        }


        if (activeCloneTasks.Count > 0)
        {
            await UniTask.WhenAll(activeCloneTasks);
        }


        for (int i = 0; i < caster.Targets.Count; i++)
        {
            clonePrefabs[i].gameObject.SetActive(false);
        }

        PutOnCooldown();
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
            //AddressablesManager.Instance.RemoveAsset(vfxRef);

        }

        var cloneRef = skillData.cloneReference;

        if(cloneRef != null)
        {
            GameObject clone = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(cloneRef);
            for (int i = 0; i < skillData.NumberClone; i++)
            {
                var clonePrefab = Object.Instantiate(clone, Vector3.zero, Quaternion.identity);
                clonePrefab.gameObject.SetActive(false);

                if (clonePrefab.GetComponent<SortingGroup>() == null)
                {
                    SortingGroup sp = clonePrefab.gameObject.AddComponent<SortingGroup>();
                    sp.sortingOrder = 0;
                }

                clonePrefabs.Add(clonePrefab);
            }

        }
    }
}

public class SummonSkillData : SkillData
{
    public Vector3 Offset = new Vector3(3.64f, 1.18f, 0);

    public Vector3 OffsetTarget = new Vector3(7, 0, 0);

    public string vfxEffectReference = "Smoke_Wukong";

    public string cloneReference = "Clone";

    public int NumberClone = 6;
    public override SkillRuntime CreateRuntimeSkill(EntityStats owner) => new SummonSkill(owner, this);
}

