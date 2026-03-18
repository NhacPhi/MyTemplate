using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;
using System.Linq;

public enum BattleResult
{
    Win,
    Lose,
    Flee // (Tùy chọn) Bỏ chạy
}
public class ResultState : BattleBaseState
{
    //1. Phân loại kết quả trận đấu
    // Update result (exp, reward)
    // ShowUI reward.
    // Clean scene back to main scene
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
        var alivePlayers = battleManager.Characters.Values
            .Where(e => !e.GetCoreComponent<EntityStats>().IsDead)
            .ToList();
    }

    private async UniTask HandleDefeatAsync()
    {

    }
}
