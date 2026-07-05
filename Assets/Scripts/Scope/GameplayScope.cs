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
        public static GameplayScope Instance { get; private set; }

        protected override void Awake()
        {
            Instance = this;
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
        //builder.Register<GameStateManager>(Lifetime.Scoped);
        builder.Register<QuestManager>(Lifetime.Singleton);
        builder.Register<DailyQuestManager>(Lifetime.Singleton);

        //Hireachy
        //builder.RegisterComponentInHierarchy<GameManager>().AsSelf();
        builder.RegisterComponentInHierarchy<GameNarrativeData>().AsSelf();
        builder.RegisterComponentInHierarchy<QuestIndicatorManager>().AsSelf();

        //Entry point
        builder.RegisterEntryPoint<GameplayPreload>(Lifetime.Singleton).As<IPreload>();
    }

    }

