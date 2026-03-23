using System.Collections;
using System.Collections.Generic;
using Tech.StateMachine;
using UnityEngine;

public class OrderState : BattleBaseState
{
    //Thu thập dữ liệu: Quét toàn bộ các nhân vật (cả Player và Enemy) đang còn sống trên sân trường.
    //Tính toán và Sắp xếp (Sorting):Speed
    // Cập nhật UI Timeline: thanh hiển thị thứ tự lượt đi
    // Chỉ định Current Unit (Nhân vật đến lượt)
    public OrderState(BattleManager battleManager) : base(battleManager) { }

    public override void Enter()
    {
        Entity nextCharacter = battleManager.TurnSystem.GetNextCharacter();

        battleManager.CurrentCaster = nextCharacter;
        
        var pos_char = battleManager.CurrentCaster.gameObject.transform.position;

        var pos = new Vector3(pos_char.x, pos_char.y - battleManager.OffsetY, pos_char.z);

        battleManager.SeletionCircle.transform.position = pos;

        if (battleManager.Boss)
            UIEvent.OnUpdateBossUI(battleManager.Boss);

        battleManager.StateMachine.ChangeState(BattleState.BeginTurnBase);

    }

    public override void Exit()
    {

    }
}
