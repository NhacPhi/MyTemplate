using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using UIFramework;

namespace Core.Scope
{
    public class GameplayScope : LifetimeScope
    {
        [SerializeField] UISettings uiSetings;
        protected override void Configure(IContainerBuilder builder)
        {
            // Data Service
            builder.Register<EventManager>(Lifetime.Scoped);
            builder.Register<SaveSystem>(Lifetime.Scoped);
            builder.Register<CurrencyManager>(Lifetime.Scoped);


            builder.RegisterComponent(uiSetings);

            // Hireachy
            builder.RegisterComponentInHierarchy<UIManager>().AsSelf();
            builder.RegisterComponentInHierarchy<SceneLoader>().AsSelf();
            builder.RegisterComponentInHierarchy<GameDataBase>().AsSelf();

            //Entry point
            builder.RegisterEntryPoint<GameplayPreLoad>(Lifetime.Scoped).As<IPreload>();
            //builder.RegisterEntryPoint<CurrencyManager>(Lifetime.Scoped).AsSelf();


            builder.RegisterBuildCallback(container =>
            {
                _ = Loading(container);
            });
        }

        private async UniTaskVoid Loading(IObjectResolver objectResolver)
        {
            var loadProgress = objectResolver.Resolve<IPreload>();

            while (!loadProgress.IsLoadDone())
            {
                await UniTask.Yield();
            }

            //PlayerCtrl = objectResolver.Instantiate(PlayerPrefab).GetComponent<PlayerCtrl>();
            //objectResolver.Resolve<TurnManager>().PlayerInfo = PlayerCtrl;
        }
    }

}
