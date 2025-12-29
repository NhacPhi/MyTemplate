using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Tech.Composite;
using UnityEngine;

public enum Skill
{
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

        SkillData dataMajor = new SummonSkillData();
        SkillRuntime majorSkill = dataMajor.CreateRuntimeSkill(entityStats);

        Skills.Add(Skill.Major, majorSkill);



        var initializer = majorSkill as IAsyncInitializer;
        if (initializer != null)
        {
            await initializer.InitializeAsync(token);
        }


        SkillData dataMain = new EmpoweredAttackData();
        SkillRuntime mainSkill = dataMain.CreateRuntimeSkill(entityStats);

        Skills.Add(Skill.Main, mainSkill);
    }
}
