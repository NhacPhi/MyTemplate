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
        UIEvent.OnActiveBossUI += ActiveBossUI;

        RefreshCurrentBattleUI();
    }

    public void RefreshCurrentBattleUI()
    {
        if (BattleManager.Instance != null)
        {
            if (BattleManager.Instance.TurnSystem != null)
            {
                var predictedOrder = BattleManager.Instance.TurnSystem.PredictTurnOrder();
                if (predictedOrder != null && predictedOrder.Count > 0)
                {
                    UpdatePredictionAvatar(predictedOrder);
                }
            }

            if (BattleManager.Instance.CurrentCaster != null)
            {
                UpdateSkillCharacterUI(BattleManager.Instance.CurrentCaster);
                SkillSwitchOnOff(BattleManager.Instance.CurrentCaster.Team == TeamSide.Player);
            }

            if (BattleManager.Instance.Boss != null)
            {
                ActiveBossUI(true);
                UIEvent.OnUpdateBossUI?.Invoke(BattleManager.Instance.Boss);
            }
            else
            {
                ActiveBossUI(false);
            }
        }
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
        if (character == null || _gameData == null) return;

        var characterConfig = _gameData.GetCharacterConfig(character.GetEntityID());
        if (characterConfig == null) return;

        // Update the large avatar (the last element in the prediction list) to match the active skill UI character
        if (_preditionAvatar != null && _preditionAvatar.Count > 0)
        {
            var largeAvatar = _preditionAvatar[_preditionAvatar.Count - 1];
            if (largeAvatar != null && characterConfig.Icon != null)
            {
                largeAvatar.sprite = characterConfig.Icon;
                largeAvatar.gameObject.SetActive(true);
            }
        }

        if (_baseSkill != null)
        {
            _baseSkill.ActiveToggle(true);
            _baseSkill.SetIconSkill(characterConfig.BaseSkillIcon);
        }
        if (_majorSkill != null)
        {
            _majorSkill.SetIconSkill(characterConfig.MajorSkillIcon);
        }
        if (_ultimateSkill != null)
        {
            _ultimateSkill.SetIconSkill(characterConfig.UltimateSkillIcon);
        }

        var skillConfig = characterConfig.Skills;
        if (skillConfig == null) return;

        var entitySkill = character.GetCoreComponent<EntitySkill>();

        foreach (var kvp in skillConfig)
        {
            SkillCharacter type = kvp.Key;
            SkillComponent data = kvp.Value;

            int currentCD = entitySkill != null ? entitySkill.GetCurrentCooldown(type) : 0;

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

    public void UpdatePredictionAvatar(List<Entity> entities)
    {
        if (entities == null || _preditionAvatar == null) return;

        // Loop only up to Count - 1, leaving the last slot (Large Avatar) for the active skill character
        int predictionSlots = _preditionAvatar.Count - 1;

        for (int i = 0; i < predictionSlots; i++)
        {
            if (i < entities.Count && entities[i] != null)
            {
                var entityID = entities[i].GetEntityID();
                var config = _gameData.GetCharacterConfig(entityID);
                if (config != null && config.Icon != null)
                {
                    _preditionAvatar[i].sprite = config.Icon;
                    _preditionAvatar[i].gameObject.SetActive(true);
                }
                else
                {
                    _preditionAvatar[i].gameObject.SetActive(false);
                }
            }
            else
            {
                _preditionAvatar[i].gameObject.SetActive(false);
            }
        }
    }

    public void SkillSwitchOnOff(bool isOn)
    {
        if (_skill != null) _skill.gameObject.SetActive(isOn);
        
        if (_preditionAvatar != null && _preditionAvatar.Count > 0)
        {
            var largeAvatar = _preditionAvatar[_preditionAvatar.Count - 1];
            if (largeAvatar != null)
            {
                largeAvatar.gameObject.SetActive(isOn);
            }
        }
    }

    public void ActiveBossUI(bool active)
    {
        _bossUI.SetActive(active);
    }
}
