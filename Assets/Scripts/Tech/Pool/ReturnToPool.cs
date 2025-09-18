using System;
using UnityEngine;

namespace Tech.Pool
{
    public class ReturnToPool : MonoBehaviour
    {
        [NonSerialized] public Pool PoolObjects;
        [NonSerialized] public Component RootCompoment;

        public void OnDisable()
        {
            if(RootCompoment)
            {
                PoolObjects.AddToPool(RootCompoment);
                return;
            }

            PoolObjects.AddToPool(gameObject);
        }
    }

}
