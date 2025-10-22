using UnityEngine;
using UnityEngine.UI;
using VContainer;
namespace MyGame
{
    public class HelloScene : MonoBehaviour
    {
        public Button btnPlay;

        [Inject]
        public void ClickButton(HelloWorldService helloWorldService)
        {
            btnPlay.onClick.AddListener(() => helloWorldService.Hello());
        }
    }
}


