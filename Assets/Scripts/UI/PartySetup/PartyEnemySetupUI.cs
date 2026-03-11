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
    
    void Start()
    {
        if(_battleSessionContext!= null)
        {
            var battleConfig = _gameData.GetBattleConfig(_battleSessionContext.PendingBattleID);

            foreach (var enemy in battleConfig.Enemies)
            {
                if (enemy.EnemyID != "")
                {
                    GameObject character = Instantiate(_characterPrefabs, transform);

                    character.transform.SetParent(transform, false);

                    character.transform.localScale = Vector3.one;

                    SetCharacterPosition(character.transform, enemy.Slot);

                    var characterConfig = _gameData.GetCharacterConfig(enemy.EnemyID);

                    var characterSlotUI = character.GetComponent<EnemySlotUI>();

                    characterSlotUI.SetupEnemySlotUI(characterConfig.Image, LocalizationManager.Instance.GetLocalizedValue(characterConfig.Name));
                }
            }

            _txtBattleName.text = LocalizationManager.Instance.GetLocalizedValue(battleConfig.Name);
        } 
    }

    private void SetCharacterPosition(Transform trans, int position)
    {
        Transform targetSlot = _slotPositions[position - 1].transform;

        trans.SetParent(targetSlot, false);

        trans.localPosition = new Vector3(0, Offset, 0);
    }
}
