using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
namespace Core.Scope
{
    public class GameplayScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Data Service
            builder.Register<EventManager>(Lifetime.Scoped);

            // Hireachy
            builder.RegisterComponentInHierarchy<UIManager>().AsSelf();
            builder.RegisterComponentInHierarchy<SceneLoader>().AsSelf();

            //Entry point

        }
    }

}
