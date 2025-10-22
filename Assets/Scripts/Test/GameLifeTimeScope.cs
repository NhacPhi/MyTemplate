using VContainer;
using VContainer.Unity;
using UnityEngine;
namespace MyGame
{
    public class GameLifeTimeScope : LifetimeScope
    {
        [SerializeField] HelloScene helloScene;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GamePresenter>();
            builder.Register<HelloWorldService>(Lifetime.Singleton);
            builder.RegisterComponent(helloScene);
        }
    }

}

