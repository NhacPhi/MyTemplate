using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using TMPro;

public class PartyEnemySetupUI : MonoBehaviour
{
    [Inject] GameDataBase _gameData;
    [Inject] BattleSessionContext _battleSessionContext;

    [SerializeField] private TextMeshProUGUI _txtBattleName;
    [SerializeField] private List<GameObject> _slotPositions;

    [SerializeField] private GameObject _characterPrefabs;

    private float Offset = 100f;
    // Track instantiated enemy objects for cleanup when battle changes
    private readonly List<GameObject> _instantiatedEnemies = new List<GameObject>();
    
    private void OnEnable()
    {
        UIEvent.OnPrepareBattleData += InitData;
    }
    
    private void OnDisable()
    {
        UIEvent.OnPrepareBattleData -= InitData;
        CleanData();
    }
// Duplicate field removed
    
    private void Start()
    {
        InitData();
    }

    // Build enemy UI for the current battle
    // Rebuild enemy UI when a new battle is prepared
    private void InitData()
    {
        CleanData();

        // Guard against missing dependencies
        if (_gameData == null)
        {
            Debug.LogError("[PartyEnemySetupUI] GameDataBase not injected.");
            return;
        }
        if (_txtBattleName == null)
        {
            // Try to locate a TextMeshProUGUI component in children as fallback
            _txtBattleName = GetComponentInChildren<TextMeshProUGUI>();
            if (_txtBattleName == null)
            {
                Debug.LogError("[PartyEnemySetupUI] TextMeshProUGUI for battle name is not assigned.");
                return;
            }
        }
        if (_battleSessionContext == null)
        {
            Debug.LogWarning("[PartyEnemySetupUI] BattleSessionContext is null. Skipping UI init.");
            return;
        }
        var battleConfig = _gameData.GetBattleConfig(_battleSessionContext.PendingBattleID);
        if (battleConfig == null)
        {
            Debug.LogError($"[PartyEnemySetupUI] No battle config found for ID '{_battleSessionContext.PendingBattleID}'.");
            return;
        }
        _txtBattleName.text = LocalizationManager.Instance.GetLocalizedValue(battleConfig.Name);

        foreach (var enemy in battleConfig.Enemies)
        {
            if (enemy.EnemyID == "") continue;

            GameObject character = Instantiate(_characterPrefabs, transform);
            _instantiatedEnemies.Add(character);
            character.transform.SetParent(transform, false);
            character.transform.localScale = Vector3.one;
            SetCharacterPosition(character.transform, enemy.Slot);

            var characterConfig = _gameData.GetCharacterConfig(enemy.EnemyID);
            var characterSlotUI = character.GetComponent<EnemySlotUI>();
            characterSlotUI.SetupEnemySlotUI(characterConfig.Image,
                LocalizationManager.Instance.GetLocalizedValue(characterConfig.Name));
        }
    }

    // Destroy previously created enemy UI objects
    private void CleanData()
    {
        foreach (var go in _instantiatedEnemies)
        {
            if (go != null) Destroy(go);
        }
        _instantiatedEnemies.Clear();
    }

    private void SetCharacterPosition(Transform trans, int position)
    {
        Transform targetSlot = _slotPositions[position - 1].transform;

        trans.SetParent(targetSlot, false);

        trans.localPosition = new Vector3(0, Offset, 0);
    }
}
