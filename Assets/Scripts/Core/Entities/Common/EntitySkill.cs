using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Tech.Composite;
using UnityEngine;

public enum Skill
{
    Base,
    Major,
    Main
}

public class EntitySkill : CoreComponent, IAsyncInitializer
{
    public Dictionary<Skill, SkillRuntime> Skills = new Dictionary<Skill, SkillRuntime>();
    //private SkillRuntime baseSkill;
    private EntityStats entityStats;

    private void Start()
    {
        //entityStats = core.GetCoreComponent<EntityStats>();
        //SkillData data = new EmpoweredAttackData();       
        //SkillData data = new SummonSkillData();
        //baseSkill = data.CreateRuntimeSkill(entityStats);
    }

    public void ExecuteMainSkill(Skill type)
    {
        Skills.GetValueOrDefault(type).Execute(GetComponent<Entity>());
    }

    public async UniTask InitializeAsync(CancellationToken token)
    {
        entityStats = gameObject.GetComponent<EntityStats>();


        //Baas Skill
        //SkillData baseAttackDta = new BaseAttackData();
        //SkillRuntime baseSkill = baseAttackDta.CreateRuntimeSkill(entityStats);
        //Skills.Add(Skill.Base, baseSkill);

        //var init = baseSkill as IAsyncInitializer;
        //if (init != null)
        //{
        //    await init.InitializeAsync(token);
        //}

        //Major Skill
        //SkillData dataMajor = new SummonSkillData();
        //SkillData dataMajor = new RingOfUniverseData();
        //SkillData dataMajor = new FireBallData(); 
        //SkillData dataMajor = new BuffShieldData();
        //SkillData dataMajor = new ThunderBallData();
        //SkillData dataMajor = new HealingData();
        //SkillData dataMajor = new MajorAttackData();
        //SkillData dataMajor = new TorandoData();
        SkillData dataMajor = new SurikenData();
        SkillRuntime majorSkill = dataMajor.CreateRuntimeSkill(entityStats);

        Skills.Add(Skill.Major, majorSkill);



        var initializer = majorSkill as IAsyncInitializer;
        if (initializer != null)
        {
            await initializer.InitializeAsync(token);
        }


        //SkillData dataMain = new EmpoweredAttackData();
        //SkillRuntime mainSkill = dataMain.CreateRuntimeSkill(entityStats);

        //SkillData dataMain = new PoisonBallData();
        SkillData dataMain = new DivineWindData();
        SkillRuntime mainSkill = dataMain.CreateRuntimeSkill(entityStats);

        var init = mainSkill as IAsyncInitializer;
        if (init != null)
        {
            await init.InitializeAsync(token);
        }


        Skills.Add(Skill.Main, mainSkill);
    }

    public void ApplyAttackSkill(ref float damage)
    {

    }

    public void ApplyDefenseSkill(ref float damage, Transform attacker)
    {
        
    }
}
