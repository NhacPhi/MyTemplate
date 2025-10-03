using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

namespace Core.Scope
{
    public class GameplayPreLoad : IAsyncStartable
    {
        [Inject] private EventManager _evnetManager;
        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            //var tasks = new List<UniTask>()
            //{

            //};
            //_eventManager.Init(tasks, cancellation);  

            //await UniTask.WhenAll(tasks);
            await UniTask.Delay(5000);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
