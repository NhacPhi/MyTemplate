using VContainer;
using VContainer.Unity;
using UnityEngine;
using Core.Scope;

public class BattleScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        //Entry point
        builder.RegisterEntryPoint<CombatText>(Lifetime.Scoped);
        builder.RegisterEntryPoint<BattleManager>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<EnemyManager>(Lifetime.Scoped).AsSelf();
        builder.RegisterEntryPoint<CharacterManager>(Lifetime.Scoped).AsSelf(); 
    }
}
