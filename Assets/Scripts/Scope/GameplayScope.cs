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
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameStateManager>(Lifetime.Scoped);

            //Hireachy
            builder.RegisterComponentInHierarchy<GameManager>().AsSelf();
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
