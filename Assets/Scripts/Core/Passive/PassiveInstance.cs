using System;
using System.Collections.Generic;
using UnityEngine;

public class PassiveInstance : IDisposable
{
    public PassiveConfig Config { get; private set; }
    private int _level;
    public int Level => _level;
    private IStatProvider _owner;

    // Danh sách các modifier (tăng chỉ số) đang được active bởi Passive này
    public List<EquipModifier> Modifiers { get; private set; } = new List<EquipModifier>();

    // Số stack tích lũy trong trận đấu (dành cho các nội tại cộng dồn theo hiệp/chịu đòn)
    public int StackCount { get; set; } = 0;

    public PassiveInstance(PassiveConfig config, int level, IStatProvider owner = null)
    {
        Config = config;
        _level = level;
        _owner = owner;
    }

    public void Activate()
    {
        ApplyStaticModifiers();
        // SubscribeCombatEvents is now handled dynamically by EntityPassive in battle
    }

    public void Dispose()
    {
        Modifiers.Clear();
    }

    private void ApplyStaticModifiers()
    {
        if (Config == null || Config.StaticModifiers == null) return;

        int index = Mathf.Max(0, _level - 1);
        foreach (var staticMod in Config.StaticModifiers)
        {
            if (Enum.TryParse(staticMod.StatType, out StatType sType) &&
                Enum.TryParse(staticMod.ModifyType, out ModifyType mType))
            {
                float valAtLevel = staticMod.ModifyByUpgrade[Mathf.Min(index, staticMod.ModifyByUpgrade.Count - 1)];
                Modifiers.Add(new EquipModifier
                {
                    Type = sType,
                    ModifierType = mType,
                    BaseValue = valAtLevel,
                    UpgradeBonus = 0
                });
            }
        }
    }

    public void SubscribeToEntity(Entity ownerEntity)
    {
        if (Config == null || Config.CombatEvents == null) return;

        foreach (var evtConfig in Config.CombatEvents)
        {
            switch (evtConfig.EventType)
            {
                case "OnTurnStart":
                    ownerEntity.OnTurnStart += (entity) => TriggerPassiveEffect(evtConfig, entity, null);
                    break;
                case "OnTakeDamage":
                    var stats = ownerEntity.GetCoreComponent<EntityStats>();
                    if (stats != null)
                    {
                        stats.OnHit += (damage, attacker, tags) => TriggerPassiveEffect(evtConfig, ownerEntity, attacker, damage, tags);
                    }
                    break;
                case "OnDeath":
                    var statsDeath = ownerEntity.GetCoreComponent<EntityStats>();
                    if (statsDeath != null)
                    {
                        statsDeath.OnDeath += () => TriggerPassiveEffect(evtConfig, ownerEntity, null);
                    }
                    break;
                case "OnAfterDealDamage":
                    ownerEntity.OnAfterDealDamage += (attacker, target, damage, tags) =>
                    {
                        var eventTags = new HashSet<string>();
                        if (tags != null)
                        {
                            foreach (var t in tags) eventTags.Add(t);
                        }

                        bool isBasic = (tags != null && tags.Contains("BasicAttack")) ||
                                       (BattleManager.Instance != null && BattleManager.Instance.CurrentSkill == SkillCharacter.Base);
                        if (!isBasic)
                        {
                            eventTags.Add("ActiveSkill");
                            eventTags.Add("IsSkill");
                        }
                        else
                        {
                            eventTags.Add("BasicAttack");
                        }

                        if (target != null)
                        {
                            var targetStats = target.GetCoreComponent<EntityStats>();
                            if (targetStats != null)
                            {
                                var hpAttr = targetStats.GetAttribute(AttributeType.Hp);
                                if (targetStats.IsDead || (hpAttr != null && hpAttr.Value <= 0))
                                {
                                    eventTags.Add("TargetDead");
                                }
                                else
                                {
                                    eventTags.Add("TargetAlive");
                                }
                            }
                        }

                        eventTags.Add("SingleTarget");
                        TriggerPassiveEffect(evtConfig, ownerEntity, target != null ? target.transform : null, damage, eventTags);
                    };
                    break;
                case "OnAfterSkillExecute":
                case "OnAfterSkillExcute":
                    ownerEntity.OnAfterSkillExecute += (entity) =>
                    {
                        GameObject tgtObj = (entity != null) ? entity.Target : null;
                        Transform targetTransform = tgtObj != null ? tgtObj.transform : null;
                        var tags = new HashSet<string>();
                        tags.Add("ActiveSkill");
                        tags.Add("IsSkill");

                        // Kiểm tra kỹ năng vừa thi triển là Đơn Mục Tiêu (SingleTarget) hay Diện Rộng (AOE)
                        bool isAOE = false;
                        if (entity != null && entity.Targets != null && entity.Targets.Count > 1)
                        {
                            isAOE = true;
                        }

                        if (BattleManager.Instance != null && entity != null)
                        {
                            var skillComp = entity.GetCoreComponent<EntitySkill>();
                            var curSkill = skillComp != null ? skillComp.GetSkill(BattleManager.Instance.CurrentSkill) : null;
                            if (curSkill != null && curSkill.GetSkillData() != null)
                            {
                                var tType = curSkill.GetSkillData().TargetType;
                                if (tType == SkillTargetType.AllEnemies || tType == SkillTargetType.AllAllies || tType == SkillTargetType.EnemyColumn || tType == SkillTargetType.EnemyRow)
                                {
                                    isAOE = true;
                                }
                            }
                        }

                        if (isAOE)
                        {
                            tags.Add("AOE");
                            tags.Add("AllEnemies");
                            tags.Add("MultiTarget");
                        }
                        else
                        {
                            tags.Add("SingleTarget");
                        }

                        if (tgtObj != null)
                        {
                            var tgtEntity = tgtObj.GetComponent<Entity>();
                            if (tgtEntity != null && tgtEntity.GetCoreComponent<EntityStats>() != null && !tgtEntity.GetCoreComponent<EntityStats>().IsDead)
                            {
                                tags.Add("TargetAlive");
                            }
                        }
                        TriggerPassiveEffect(evtConfig, ownerEntity, targetTransform, 0f, tags);
                    };
                    break;
            }
        }
    }

    public void UnsubscribeFromEntity(Entity ownerEntity)
    {
        // Unsubscribe logic should ideally store the generated delegates to safely remove them (-=)
        // For simplicity in this structure without storing delegates, if Entity is destroyed, events are cleared anyway.
        // But if you re-pool entities, you MUST unsubscribe explicitly.
    }

    private void TriggerPassiveEffect(CombatEventConfig evtConfig, Entity owner, Transform attackerTarget, float value = 0f, System.Collections.Generic.HashSet<string> tags = null)
    {
        // Lấy Entity mục tiêu nếu có
        var targetEntity = attackerTarget != null ? attackerTarget.GetComponent<Entity>() : null;
        
        // 1. Tạo CombatContext đóng gói thông tin trận đấu thực tế bằng Constructor của nó
        // Gắn thêm 'value' (chính là lượng sát thương/hồi máu) vào Context
        var context = new CombatContext(owner, targetEntity, value);

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                context.AddTag(tag);
            }
        }

        // 2. Giao phó toàn bộ việc Kiểm tra (Matching) & Thực thi (Execution) cho PassiveEventListener
        PassiveEventListener.EvaluateAndExecute(evtConfig, _level, context);
    }

    public float GetCombatEventValue(int eventIndex = 0)
    {
        if (Config == null || Config.CombatEvents == null || Config.CombatEvents.Count <= eventIndex) return 0f;
        var ce = Config.CombatEvents[eventIndex];
        if (ce.ModifyByUpgrade == null || ce.ModifyByUpgrade.Count == 0) return ce.EffectParam;
        int idx = Mathf.Clamp(_level - 1, 0, ce.ModifyByUpgrade.Count - 1);
        return ce.ModifyByUpgrade[idx];
    }

    // Condition trigger passive event
    // Passive EDA (T-C-T-E Trigger, Condition, Target, Effect)
    // Target (Base: Self, Target, Source Ap dung ke dich gay sat thuong cho minhf)
    //        (AOE: AllAllies, OtherAllies (k bao gom minh) AllEnemies
    //        (Advance: LowestHPAlly, RandomEnemy or HighestATKAlly)
}
