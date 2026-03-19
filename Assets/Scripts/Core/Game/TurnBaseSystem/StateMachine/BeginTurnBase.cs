using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class BeginTurnBase : BattleBaseState
{
    // Effect Xử lý sát thương / Hồi phục theo thời gian
    //Kiểm tra hiệu ứng khống chế (Crowd Control)
    //Kích hoạt Kỹ năng Bị động (Passive Skills)
    public BeginTurnBase(BattleManager battleManager) : base(battleManager) { }

    public override void Enter()
    {
        var skillManager = battleManager.CurrentCharacter.GetCoreComponent<EntitySkill>();

        if (skillManager != null)
        {
            skillManager.TickCooldowns();
        }

        UIEvent.OnSwithActiveSkilCharacter?.Invoke(true);

        UIEvent.OnUpdateSkillCharacterUI?.Invoke(battleManager.CurrentCharacter);

        battleManager.CurrentSkill = SkillCharacter.Base;

        battleManager.StateMachine.ChangeState(BattleState.ActionState);
    }

    public override void Exit()
    {

    }
}
