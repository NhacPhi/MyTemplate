using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class SkillCharacterUIManager : MonoBehaviour
{
    [SerializeField] private ToggleSkillCharacterUI _baseSkill;

    [SerializeField] private ToggleSkillCharacterUI _majorSkill;

    [SerializeField] private ToggleSkillCharacterUI _ultimateSkill;

    [SerializeField] private List<Image> _preditionAvatar;

    [SerializeField] private GameObject _skill;

    [SerializeField] private GameObject _bossUI;
    [Inject] private GameDataBase _gameData;
    private void OnEnable()
    {
        UIEvent.OnUpdateSkillCharacterUI += UpdateSkillCharacterUI;
        UIEvent.OnUpdateEntityPrediction += UpdatePredictionAvatar;
        UIEvent.OnSwithActiveSkilCharacter += SkillSwitchOnOff;
        UIEvent.OnActiveBossUI+= ActiveBossUI;
    }

    private void OnDisable()
    {
        UIEvent.OnUpdateSkillCharacterUI -= UpdateSkillCharacterUI;
        UIEvent.OnUpdateEntityPrediction -= UpdatePredictionAvatar;
        UIEvent.OnSwithActiveSkilCharacter -= SkillSwitchOnOff;
        UIEvent.OnActiveBossUI -= ActiveBossUI;
    }

    public void UpdateSkillCharacterUI(Entity character)
    {

        var characterConfig = _gameData.GetCharacterConfig(character.GetEntityID());

        _baseSkill.gameObject.GetComponent<ToggleBase>().ActiveToggle(true);

        var baseSkill = characterConfig.BaseSkillIcon;
        var majorSkill = characterConfig.MajorSkillIcon;
        var ultimateSKill = characterConfig.UltimateSkillIcon;

        _baseSkill.SetIconSkill(baseSkill);
        _majorSkill.SetIconSkill(majorSkill);
        _ultimateSkill.SetIconSkill(ultimateSKill);

        var skillConfig = characterConfig.Skills;

        foreach (var kvp in skillConfig)
        {
            SkillCharacter type = kvp.Key;
            SkillComponent data = kvp.Value;

            int currentCD = character.GetCoreComponent<EntitySkill>().GetCurrentCooldown(type);

            switch (type)
            {
                case SkillCharacter.Base:
                    _baseSkill.UpdateSkillUI(data, currentCD);
                    break;
                case SkillCharacter.Major:
                    _majorSkill.UpdateSkillUI(data, currentCD);
                    break;
                case SkillCharacter.Ultimate:
                    _ultimateSkill.UpdateSkillUI(data, currentCD);
                    break;
            }
        }
    }

    public void UpdatePredictionAvatar(List<Entity> entities)
    {
        for(int i = 0; i < entities.Count; i++)
        {
            var entityID = entities[i].GetEntityID();
            _preditionAvatar[i].sprite = _gameData.GetCharacterConfig(entityID).Icon;
        }

        //var index = entities.Count;

        //if (index == _preditionAvatar.Count) return;

        //for(int i = index; i <_preditionAvatar.Count; i++)
        //{
        //    _preditionAvatar[i].gameObject.SetActive(false);
        //}
    }

    public void SkillSwitchOnOff(bool isOn)
    {
        _skill.gameObject.SetActive(isOn);
    }

    public void ActiveBossUI(bool active)
    {
        _bossUI.SetActive(active);
    }
}
