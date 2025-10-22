using UnityEngine;
using VContainer.Unity;
using VContainer;
namespace MyGame
{
    public class GamePresenter : IStartable
    {
        //readonly HelloWorldService helloWorldService;
        //readonly HelloScene helloScene;

        [Inject] private HelloWorldService helloWorldService;

        //public GamePresenter(HelloWorldService helloWorldService, HelloScene helloScene)
        //{
        //    this.helloWorldService = helloWorldService;
        //    this.helloScene = helloScene;
        //}

        void IStartable.Start()
        {
            //helloScene.btnPlay.onClick.AddListener(() => helloWorldService.Hello());
            //DoSomething();
        }
        public void DoSomething()
        {
            helloWorldService.Hello();
        }
    }
}



