using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Singleton;
using VContainer;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace Tech.Pool
{
    public enum PoolType
    {
        Projectile,
        Enemy,
        VFX,
        None
    }

    public class PoolManager : Singleton<PoolManager>
    {
        [Inject] private IObjectResolver _objectResolver;
        private readonly Dictionary<Object, Pool> _objectPools = new();
        private readonly Dictionary<PoolType, Transform> _poolsHolder = new();

        protected override void Awake()
        {
            base.Awake();
            SetupHolder();
        }

        private void SetupHolder()
        {
            GameObject holder = new GameObject("Pool Holder");
            holder.transform.SetParent(transform);
            var child = new Transform[transform.childCount];

            for(int i = 0; i < transform.childCount; i++)
            {
                child[i] = transform.GetChild(i);
            }

            foreach(PoolType pool in Enum.GetValues(typeof(PoolType)))
            {
                if(pool == PoolType.None) continue;
                 
                var poolName = pool.ToString();

                Transform exitTransform = child.FirstOrDefault(x => x.name == poolName);

                if(exitTransform)
                {
                    _poolsHolder.Add(pool, exitTransform);
                    continue;
                }

                GameObject empty = new(poolName);
                empty.transform.SetParent(holder.transform);
                _poolsHolder.Add(pool, empty.transform);
            }
        }

        public GameObject SpawnObject(GameObject objectToSpawn, Vector3 postion, Quaternion rotaion, PoolType poolType = PoolType.None)
        {
           var spawnableObject = Spawn(objectToSpawn, postion, rotaion, poolType);

            if(poolType != PoolType.None)
            {
                spawnableObject.transform.SetParent(GetPoolParent(poolType).transform);
            }

            return spawnableObject;
        }

        public T SpawnObject<T>(T objectToSpawn, Vector3 postion, Quaternion rotaion
            , PoolType poolType = PoolType.None) where T : Component
        {
            var spawnableObject = Spawn(objectToSpawn, postion, rotaion, poolType);

            if (poolType != PoolType.None)
            {
                spawnableObject.transform.SetParent(GetPoolParent(poolType).transform);
            }

            return spawnableObject;
        }
        private T Spawn<T>(T obj, Vector3 position, Quaternion rotaion, PoolType poolType = PoolType.None) where T : Object
        {
            if(!_objectPools.ContainsKey(obj))
            {
                _objectPools.Add(obj, new Pool(obj));
            }

            T spawnableObject = _objectPools[obj].GetPool(_objectResolver, position, rotaion) as T;

            return spawnableObject;
        }

        public void ClearnPool(bool includePersistent)
        {
            if (!includePersistent) return;
            _objectPools.Clear();
        }

        public GameObject GetPoolParent(PoolType poolType)
        {
            return _poolsHolder[poolType].gameObject;
        }
    }
}
