using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using UIFramework;

    public class GameplayScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            //builder.Register<GameStateManager>(Lifetime.Scoped);

            //Hireachy
            //builder.RegisterComponentInHierarchy<GameManager>().AsSelf();
            //builder.RegisterComponentInHierarchy<GameNarrativeData>().AsSelf();
        }

    }

