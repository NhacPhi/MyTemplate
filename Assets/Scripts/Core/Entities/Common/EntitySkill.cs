using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Tech.Composite;
using UnityEngine;
using VContainer;
public enum SkillCharacter
{
    Base,
    Major,
    Ultimate
}

public enum Skill
{
    None,
    Melee,
    Range,
    MajorAttack,
    Summon,
    BuffShield,
    Healing,
    FireRing,
    ThunderBall,
    FireBall,
    EmpowerAttack,
    Torando,
    Suriken,
    PosionBall,
    DivineWind
}

public class EntitySkill : CoreComponent, IAsyncInitializer
{
    public Dictionary<SkillCharacter, SkillRuntime> Skills = new Dictionary<SkillCharacter, SkillRuntime>();
    //private SkillRuntime baseSkill;
    private EntityStats entityStats;
    [Inject] GameDataBase _gameDataBase;
    private void Start()
    {
        //entityStats = core.GetCoreComponent<EntityStats>();
        //SkillData data = new EmpoweredAttackData();       
        //SkillData data = new SummonSkillData();
        //baseSkill = data.CreateRuntimeSkill(entityStats);
    }

    public async UniTask ExecuteSkillAsync(SkillCharacter type)
    {
        await Skills.GetValueOrDefault(type).ExecuteAsync(GetComponent<Entity>());
    }

    public async UniTask InitializeAsync(CancellationToken token)
    {
        entityStats = gameObject.GetComponent<EntityStats>();

        var characterConfig = _gameDataBase.GetCharacterConfig(entityStats.EntityID);

        // Base Skill
        var baseSkillID = characterConfig.Skills.GetValueOrDefault(SkillCharacter.Base);
        SkillData dataBaseSkill = SkillDataFactory.Create(baseSkillID);

        SkillRuntime baseSkill = dataBaseSkill.CreateRuntimeSkill(entityStats);

        var initializerBase = baseSkill as IAsyncInitializer;

        if (initializerBase != null)
        {
            await initializerBase.InitializeAsync(token);
        }

        Skills.Add(SkillCharacter.Base, baseSkill);


        //Majojr Skill 
        var majorSkillID = characterConfig.Skills.GetValueOrDefault(SkillCharacter.Major);

        SkillData dataMajor = SkillDataFactory.Create(majorSkillID);

        SkillRuntime majorSkill = dataMajor.CreateRuntimeSkill(entityStats);

        var initializerMajor = majorSkill as IAsyncInitializer;

        if (initializerMajor != null)
        {
            await initializerMajor.InitializeAsync(token);
        }

        Skills.Add(SkillCharacter.Major, majorSkill);

        // Ultimate Skill
        var ultimateSkillID = characterConfig.Skills.GetValueOrDefault(SkillCharacter.Ultimate);

        SkillData dataUltiamte = SkillDataFactory.Create(ultimateSkillID);

        SkillRuntime ultimateSkill = dataUltiamte.CreateRuntimeSkill(entityStats);

        var initializerUltimate = ultimateSkill as IAsyncInitializer;

        if (initializerUltimate != null)
        {
            await initializerUltimate.InitializeAsync(token);
        }

        Skills.Add(SkillCharacter.Ultimate, ultimateSkill);
    }

    public void ApplyAttackSkill(ref float damage)
    {

    }

    public void ApplyDefenseSkill(ref float damage, Transform attacker)
    {
        
    }
}
