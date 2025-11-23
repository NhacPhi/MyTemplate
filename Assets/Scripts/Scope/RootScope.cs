using Core.Scope;
using System.Collections;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootScope : LifetimeScope
{
    [SerializeField] UISettings uiSetings;

    protected override void Configure(IContainerBuilder builder)
    {
        // Data Service
        builder.Register<EventManager>(Lifetime.Singleton);
        builder.Register<SaveSystem>(Lifetime.Singleton);
        builder.Register<CurrencyManager>(Lifetime.Singleton);
        builder.Register<CharacterStatManager>(Lifetime.Singleton);
        //builder.Register<GameNarrativeData>(Lifetime.Singleton);

        builder.RegisterComponent(uiSetings);

        // Hireachy
        builder.RegisterComponentInHierarchy<UIManager>().AsSelf();
        builder.RegisterComponentInHierarchy<SceneLoader>().AsSelf();
        builder.RegisterComponentInHierarchy<GameDataBase>().AsSelf();
        builder.RegisterComponentInHierarchy<GameNarrativeData>().AsSelf();

        //Entry point
        builder.RegisterEntryPoint<GameplayPreLoad>(Lifetime.Singleton).As<IPreload>();

    }
}
