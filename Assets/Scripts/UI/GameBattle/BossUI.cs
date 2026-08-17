using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using TMPro;

public class BossUI : MonoBehaviour
{
    [SerializeField] private SkillBossUI _baseSkill;

    [SerializeField] private SkillBossUI _majorSkill;

    [SerializeField] private SkillBossUI _ultimateSkill;

    [SerializeField] private TextMeshProUGUI _txtBossName;

    [SerializeField] private Image _bossAvatar;

    [SerializeField] Slider _bossHP;

    [Inject] private GameDataBase _gameData;

    private void Start()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.CheckBattleHasBosss();
            gameObject.SetActive(BattleManager.Instance.Boss != null);
        }
    }

    private void OnEnable()
    {
        UIEvent.OnUpdateBossUI += UpdateSkillBossUI;
    }

    private void OnDisable()
    {
        UIEvent.OnUpdateBossUI -= UpdateSkillBossUI;
    }

    public void UpdateSkillBossUI(Entity boss)
    {
        if (boss == null) return;
        var characterConfig = _gameData.GetCharacterConfig(boss.GetEntityID());
        if (characterConfig == null) return;

        var avatar = characterConfig.Icon;

        var baseSkill = characterConfig.BaseSkillIcon;
        var majorSkill = characterConfig.MajorSkillIcon;
        var ultimateSKill = characterConfig.UltimateSkillIcon;

        if (_baseSkill != null) _baseSkill.SetIconSkill(baseSkill);
        if (_majorSkill != null) _majorSkill.SetIconSkill(majorSkill);
        if (_ultimateSkill != null) _ultimateSkill.SetIconSkill(ultimateSKill);

        var skillConfig = characterConfig.Skills;

        if (skillConfig != null)
        {
            foreach (var kvp in skillConfig)
            {
                SkillCharacter type = kvp.Key;
                SkillComponent data = kvp.Value;

                var entitySkill = boss.GetCoreComponent<EntitySkill>();
                if (entitySkill == null) continue;

                int currentCD = entitySkill.GetCurrentCooldown(type);

                switch (type)
                {
                    case SkillCharacter.Base:
                        if (_baseSkill != null) _baseSkill.UpdateSkillUI(data, currentCD);
                        break;
                    case SkillCharacter.Major:
                        if (_majorSkill != null) _majorSkill.UpdateSkillUI(data, currentCD);
                        break;
                    case SkillCharacter.Ultimate:
                        if (_ultimateSkill != null) _ultimateSkill.UpdateSkillUI(data, currentCD);
                        break;
                }
            }
        }

        if (_txtBossName != null && characterConfig.Name != null)
            _txtBossName.text = LocalizationManager.Instance.GetLocalizedValue(characterConfig.Name);

        var entityStats = boss.GetCoreComponent<EntityStats>();
        if (entityStats != null && _bossHP != null)
        {
            var characterHp = entityStats.GetAttribute(AttributeType.Hp);
            if (characterHp != null)
            {
                _bossHP.minValue = 0;
                _bossHP.maxValue = characterHp.MaxValue;
                _bossHP.value = characterHp.Value;
            }
        }

        if (_bossAvatar != null)
            _bossAvatar.sprite = avatar;

    }
}
