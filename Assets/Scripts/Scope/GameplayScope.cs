using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using UIFramework;
using Core.Scope;

    public class GameplayScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
        //builder.Register<GameStateManager>(Lifetime.Scoped);
        builder.Register<QuestManager>(Lifetime.Singleton);

        //Hireachy
        //builder.RegisterComponentInHierarchy<GameManager>().AsSelf();
        //builder.RegisterComponentInHierarchy<GameNarrativeData>().AsSelf();

        //Entry point
        builder.RegisterEntryPoint<GameplayPreload>(Lifetime.Singleton).As<IPreload>();
    }

    }

