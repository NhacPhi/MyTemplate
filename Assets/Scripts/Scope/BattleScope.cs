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

        //builder.RegisterComponentInHierarchy<BattleManager>().AsSelf();
        builder.RegisterComponentInHierarchy<BattleManager>().AsImplementedInterfaces();
    }
}
