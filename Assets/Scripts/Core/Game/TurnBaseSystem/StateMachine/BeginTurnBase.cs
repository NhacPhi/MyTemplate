using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tech.StateMachine;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BeginTurnBase : BattleBaseState
{
    // Effect Xử lý sát thương / Hồi phục theo thời gian
    //Kiểm tra hiệu ứng khống chế (Crowd Control)
    //Kích hoạt Kỹ năng Bị động (Passive Skills)
    public BeginTurnBase(BattleManager battleManager) : base(battleManager) { }

    public override async void Enter()
    {
        try
        {
            var enemyBrain = battleManager.CurrentCaster.GetCoreComponent<EnemyBrain>();
            bool isEnemy = battleManager.CurrentCaster.Team == TeamSide.Enemy || enemyBrain != null;

            // Immediately show or hide Skill UI depending on whether it is Enemy or Player turn
            UIEvent.OnSwithActiveSkilCharacter?.Invoke(!isEnemy);

            // Effect Xử lý sát thương / Hồi phục theo thời gian
            var stats = battleManager.CurrentCaster.GetCoreComponent<StatsController>();
            bool skipTurn = !stats.CanTakeTurn();
            stats.ProcessStartOfTurn();

            if (battleManager.CurrentCaster.GetCoreComponent<EntityStats>().IsDead)
            {
                battleManager.StateMachine.ChangeState(BattleState.EndTurnState);
                return;
            }

            // 4. KIỂM TRA KHỐNG CHẾ (CHOÁNG, NGỦ, ĐÓNG BĂNG...)
            if (skipTurn)
            {
                // Hiển thị chữ mất lượt địa phương hóa lên đầu nhân vật
                string skipText = LocalizationManager.Instance.GetLocalizedValue("STR_SKIP_TURN");
                UIEvent.TextPopup?.Invoke(skipText, battleManager.CurrentCaster.transform.position);

                battleManager.StateMachine.ChangeState(BattleState.EndTurnState);
                return;
            }

            // Skil 
            var skillManager = battleManager.CurrentCaster.GetCoreComponent<EntitySkill>();

            if (skillManager != null)
            {
                skillManager.TickCooldowns();

                if (battleManager != null)
                {
                    await UniTask.Delay(1000, ignoreTimeScale: true, cancellationToken: battleManager.DestroyCancellationToken);
                }
            }

            if (battleManager == null || battleManager.CurrentCaster == null) return;

            if (battleManager.Boss)
                UIEvent.OnUpdateBossUI?.Invoke(battleManager.Boss);

            UIEvent.OnUpdateEntityPrediction?.Invoke(battleManager.TurnSystem.PredictTurnOrder());

            if (isEnemy)
            {
                //Enemy
                var playerTeam = battleManager.TargetSystem.GetValidEtitiesByColumnLogic(battleManager.Characters.Values.ToList());

                EnemyDecision decision;
                if (enemyBrain != null)
                {
                    decision = await enemyBrain.DecideAsync(playerTeam);
                }
                else
                {
                    Debug.LogWarning($"[BeginTurnBase] Enemy '{battleManager.CurrentCaster.name}' không có script EnemyBrain! Tự động chọn đòn đánh cơ bản.");
                    var aliveTargets = playerTeam.Where(p => p != null && p.GetCoreComponent<EntityStats>() != null && !p.GetCoreComponent<EntityStats>().IsDead).ToList();
                    Entity target = aliveTargets.Count > 0 ? aliveTargets[UnityEngine.Random.Range(0, aliveTargets.Count)] : null;
                    decision = new EnemyDecision { SkillType = SkillCharacter.Base, Target = target };
                }

                if (battleManager == null || battleManager.CurrentCaster == null) return;

                // 3. Setup dữ liệu để chuẩn bị chém
                battleManager.CurrentSkill = decision.SkillType;
                battleManager.SetCurrentTarget(decision.Target);
                battleManager.StateMachine.ChangeState(BattleState.ExecutionState);
            }
            else
            {
                //player
                UIEvent.OnSwithActiveSkilCharacter?.Invoke(true);
                UIEvent.OnUpdateSkillCharacterUI?.Invoke(battleManager.CurrentCaster);
                battleManager.SetupCurrentSkillCaster(SkillCharacter.Base);
                battleManager.StateMachine.ChangeState(BattleState.ActionState);
            }
        }
        catch (System.OperationCanceledException)
        {
            return;
        }
        catch (UnityEngine.MissingReferenceException)
        {
            return;
        }
        catch (System.ObjectDisposedException)
        {
            return;
        }
        catch (System.Exception ex)
        {
            string casterName = (battleManager != null && battleManager.CurrentCaster != null) ? battleManager.CurrentCaster.name : "Unknown";
            Debug.LogError($"[BeginTurnBase] Lỗi khi xử lý lượt của '{casterName}': {ex}");
            if (battleManager != null && battleManager.StateMachine != null)
            {
                battleManager.StateMachine.ChangeState(BattleState.EndTurnState);
            }
        }
    }

    public override void Exit()
    {

    }
}
