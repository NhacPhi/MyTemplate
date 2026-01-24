using VContainer;
using VContainer.Unity;
using UnityEngine;
using Core.Scope;

public class BattleScope : LifetimeScope
{
    [SerializeField] CharacterStats character;
    [SerializeField] EnemyStats enemy;

    //[SerializeField] CharacterStats enemy;
    protected override void Configure(IContainerBuilder builder)
    {
        //Entry point
        builder.RegisterEntryPoint<CombatText>(Lifetime.Scoped);

        //builder.RegisterComponentInHierarchy<BattleManager>().AsSelf();
        builder.RegisterComponentInHierarchy<BattleManager>().AsImplementedInterfaces();


        // temporary
        //builder.Register<AtlasProvider>(Lifetime.Singleton);
        //builder.Register<GameDataBase>(Lifetime.Singleton);
        ////Entry point
        //builder.RegisterEntryPoint<BattlePreLoad>(Lifetime.Singleton).As<IPreload>();
        //builder.RegisterComponentInHierarchy<StatsController>();
        builder.RegisterComponent(character);

        builder.RegisterComponent(enemy);
    }
}
