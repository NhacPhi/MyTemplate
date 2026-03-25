using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Tech.Composite;
using UnityEngine;
using VContainer;
using System.Linq;

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
    StatModifier,
    Healing,
    FireRing,
    ThunderBall,
    FireBall,
    EmpowerAttack,
    Torando,
    Suriken,
    PoisonBall,
    DivineWind
}

public enum SkillTargetType
{
    // Single Target
    SingleEnemy, // Áp dụng 1 kẻ địch
    SingleAlly, // Áp dụng 1 đồng minh
    Self, // Chỉ Tác dụng lên mịnh

    // Full Aoe
    AllEnemies,
    AllAllies,

    //Position Aoe
    EnemyColumn,
    EnemyRow,

    // Special
    DeadAlly // Hồi sinh
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

    public async UniTask ExecuteSkillAsync(SkillCharacter type, int currentTurnID)
    {
        await Skills.GetValueOrDefault(type).ExecuteAsync(core as Entity, currentTurnID);
    }

    public async UniTask InitializeAsync(CancellationToken token)
    {
        entityStats = gameObject.GetComponent<EntityStats>();

        var characterConfig = _gameDataBase.GetCharacterConfig(entityStats.EntityID);
        foreach(var kvp in characterConfig.Skills)
        {
            SkillCharacter skillType = kvp.Key;
            SkillComponent skillConfig = kvp.Value;

            if(skillConfig.Skill != Skill.None)
            {
                SkillData skillData = SkillDataFactory.Create(skillConfig.Skill);

                skillData.ID = skillConfig.ID;

                skillData.TargetType = skillConfig.TargetType;

                skillData.DamageMultiplier = skillConfig.DamageMultiplier;

                skillData.MaxCoolDown = skillConfig.MaxCooldown;

                skillData.FlatDamage = skillConfig.FlatDamage;

                skillData.Sound = skillConfig.Sound;

                if(skillConfig.EffectID != null)
                {
                    skillData.Effect = _gameDataBase.GetEffectConfig(skillConfig.EffectID);
                }
                else
                {
                    skillData.Effect = null;
                }

                SkillRuntime skillRuntime = skillData.CreateRuntimeSkill(entityStats);

                var initializer = skillRuntime as IAsyncInitializer;

                if (initializer != null)
                {
                    await initializer.InitializeAsync(token);
                }

                Skills.Add(skillType, skillRuntime);
            } 
        }    
    }

    public void ApplyAttackSkill(ref float damage)
    {

    }

    public void ApplyDefenseSkill(ref float damage, Transform attacker)
    {
        
    }

    public void TickCooldowns()
    {
        foreach (var runtime in Skills.Values)
        {
            runtime.TickCooldown();
        }
    }

    public SkillRuntime GetSkill(SkillCharacter type)
    {
        if (Skills.TryGetValue(type, out SkillRuntime runtime))
        {
            return runtime;
        }
        return null;
    }

    public bool IsSkillReady(SkillCharacter type)
    {
        var skill = GetSkill(type);
        if (skill != null)
        {
            return skill.IsReady();
        }
        return false;
    }

    public int GetCurrentCooldown(SkillCharacter type)
    {
        var skill = GetSkill(type);
        if (skill != null)
        {
            return skill.CurrentCooldown;
        }
        return 0;
    }

    public EntityState MatchSkillCharacterToEntityState(SkillRuntime skill)
    {
        var type = Skills.FirstOrDefault(x => x.Value == skill).Key;
        switch (type)
        {
            case SkillCharacter.Base:
                return EntityState.ATTACK;
            case SkillCharacter.Major:
                return EntityState.MAJOR_SKILL;
            case SkillCharacter.Ultimate:
                return EntityState.MAIN_SKILL;
        }
        return EntityState.ATTACK;
    }
}
