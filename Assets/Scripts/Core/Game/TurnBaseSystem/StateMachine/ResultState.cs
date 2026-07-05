using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;
using System.Linq;

public enum BattleResult
{
    Win,
    Lose,
    Flee 
}

public class ResultState : BattleBaseState
{
    private BattleResult _result;
    public ResultState(BattleManager battleManager) : base(battleManager) { }

    public override async void Enter()
    {
        _result = battleManager.ResultBattle;
        battleManager.ActionQueue.Clear();

        await UniTask.Delay(1000);

        if (_result == BattleResult.Win)
        {
            await HandleVictoryAsync();
        }
        else if (_result == BattleResult.Lose)
        {
            await HandleDefeatAsync();
        }

        UIEvent.OnShowBattleResultUI?.Invoke(_result);
    }

    public override void Exit()
    {

    }

    private async UniTask HandleVictoryAsync()
    {
        await ProcessResult(true);
    }

    private async UniTask HandleDefeatAsync()
    {
        await ProcessResult(false);
    }

    private async UniTask ProcessResult(bool isWin)
    {
        // 1. Gather MVP
        string mvpId = "";
        float maxDamage = -1;
        var characterResults = new List<CharacterResultData>();

        foreach (var kvp in battleManager.Characters)
        {
            string charId = kvp.Key;
            var entity = kvp.Value;
            var stats = entity.GetCoreComponent<EntityStats>();
            
            float damageDealt = stats.TotalDamageDealt;
            if (damageDealt > maxDamage)
            {
                maxDamage = damageDealt;
                mvpId = charId;
            }

            int currentHp = Mathf.RoundToInt(stats.GetAttribute(AttributeType.Hp).Value);
            int maxHp = Mathf.RoundToInt(stats.GetAttribute(AttributeType.Hp).MaxValue);

            characterResults.Add(new CharacterResultData
            {
                CharacterId = charId,
                CurrentHp = currentHp,
                MaxHp = maxHp
            });
        }

        // 2. EXP Calculation (Dummy values or fetch from SaveSystem)
        var account = battleManager.SaveSystem.Player.Account;
        int currentLevel = account.Level;
        int expBefore = account.CurrentExp;
        int maxExpForLevel = currentLevel * 1000; // Formula dummy
        
        int expAdded = 0;
        string rewardId = "";

        if (isWin)
        {
            var battleConfig = battleManager.GameDataBase.GetBattleConfig(battleManager.BattleSession.PendingBattleID);
            if (battleConfig != null)
            {
                expAdded = battleConfig.ExpReward;
                rewardId = battleConfig.Reward;
            }
            else
            {
                expAdded = 500; // Fallback
            }

            account.CurrentExp += expAdded;
            if (account.CurrentExp >= maxExpForLevel)
            {
                // Simple level up logic
                account.Level++;
                account.CurrentExp -= maxExpForLevel;
            }
            
            GameEvent.OnWinBattle?.Invoke(battleManager.BattleSession.PendingBattleID, 1);
            
            battleManager.SaveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
        }

        // 3. Show Result Popup
        var properties = new BattleResultProperties(
            isWin,
            mvpId,
            currentLevel,
            expBefore,
            expAdded,
            maxExpForCurrentLevel: maxExpForLevel,
            characterResults,
            onNextClicked: () =>
            {
                battleManager.UIManager.CloseWindowScene(ScreenIds.PopupBattleResult);
                
                if (isWin && !string.IsNullOrEmpty(rewardId))
                {
                    // Call Reward Popup logic here
                    var rewardConfig = battleManager.GameDataBase.GetRewardConfig(rewardId);
                    if (rewardConfig != null)
                    {
                        var rewardItems = new List<RewardItemData>();
                        foreach (var r in rewardConfig.Rewards)
                        {
                            rewardItems.Add(new RewardItemData(r.ItemID, r.Amount));
                            
                            var itemConfig = battleManager.GameDataBase.GetItemConfig(r.ItemID);
                            if (itemConfig != null)
                            {
                                if (itemConfig.Type == ItemType.Currency)
                                {
                                    if (System.Enum.TryParse<CurrencyType>(r.ItemID, true, out var currencyType))
                                    {
                                        battleManager.CurrencyManager.Add(currencyType, r.Amount);
                                    }
                                }
                                else if (itemConfig.Type == ItemType.Weapon)
                                {
                                    for (int i = 0; i < r.Amount; i++)
                                    {
                                        var newWeapon = new WeaponSaveData
                                        {
                                            UUID = System.Guid.NewGuid().ToString(),
                                            TemplateID = r.ItemID,
                                            CurrentLevel = 1
                                        };
                                        battleManager.InventoryManager.AddWeapon(newWeapon);
                                    }
                                }
                                else if (itemConfig.Type == ItemType.Armor)
                                {
                                    for (int i = 0; i < r.Amount; i++)
                                    {
                                        var newArmor = new ArmorSaveData
                                        {
                                            UUID = System.Guid.NewGuid().ToString(),
                                            TemplateID = r.ItemID,
                                            Level = 1
                                        };
                                        battleManager.InventoryManager.AddArmor(newArmor);
                                    }
                                }
                                else
                                {
                                    battleManager.InventoryManager.AddStackableItem(r.ItemID, itemConfig.Type, r.Amount);
                                }
                            }
                        }

                        var popupProps = new ReceiveItemProperties(rewardItems, () => 
                        {
                            battleManager.UIManager.OpenWindowScene(ScreenIds.GamePlayScene);
                            if (battleManager.BattleSession.PreviousLocation != null)
                            {
                                battleManager.SceneLoader.LoadLocation(battleManager.BattleSession.PreviousLocation, true, true);
                            }
                        });
                        battleManager.UIManager.ShowReceiveItemPopup(popupProps);
                    }
                    else
                    {
                        battleManager.UIManager.OpenWindowScene(ScreenIds.GamePlayScene);
                        if (battleManager.BattleSession.PreviousLocation != null)
                        {
                            battleManager.SceneLoader.LoadLocation(battleManager.BattleSession.PreviousLocation, true, true);
                        }
                    }
                }
                else
                {
                    // Lose -> back to previous location
                    battleManager.UIManager.OpenWindowScene(ScreenIds.GamePlayScene);
                    if (battleManager.BattleSession.PreviousLocation != null)
                    {
                        battleManager.SceneLoader.LoadLocation(battleManager.BattleSession.PreviousLocation, true, true);
                    }
                }
            }
        );

        battleManager.UIManager.ShowBattleResultPopup(properties);
    }
}
