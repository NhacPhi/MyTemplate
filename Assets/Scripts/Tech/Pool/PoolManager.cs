using System;
using System.Collections.Generic;
using System.Linq;
using Tech.Singleton;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Tech.Pool
{
    public enum PoolType
    {
        Projectile,
        VFX,
        Enemy,
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

            for (int i = 0; i < transform.childCount; i++)
            {
                child[i] = transform.GetChild(i);
            }

            foreach (PoolType pool in Enum.GetValues(typeof(PoolType)))
            {
                if (pool == PoolType.None) continue;

                var poolName = pool.ToString();

                Transform existTransform = child.FirstOrDefault(x => x.name == poolName);

                if (existTransform)
                {
                    _poolsHolder.Add(pool, existTransform);
                    continue;
                }

                GameObject empty = new(poolName);
                empty.transform.SetParent(holder.transform);
                _poolsHolder.Add(pool, empty.transform);
            }
        }

        public GameObject SpawnObject(GameObject objectToSpawn, Vector3 position, Quaternion rotation, PoolType poolType = PoolType.None)
        {
            var spawnableObj = Spawn(objectToSpawn, position, rotation, poolType);

            if (poolType != PoolType.None)
            {
                spawnableObj.transform.SetParent(GetPoolParent(poolType).transform);
            }

            return spawnableObj;
        }

        public T SpawnObject<T>(T objectToSpawn, Vector3 position, Quaternion rotation,
            PoolType poolType = PoolType.None) where T : Component
        {
            var spawnableObj = Spawn(objectToSpawn, position, rotation, poolType);

            if (poolType != PoolType.None)
            {
                spawnableObj.transform.SetParent(GetPoolParent(poolType).transform);
            }
            if (objectToSpawn == null)
            {
                return null;
            }
            return spawnableObj;
        }

        private T Spawn<T>(T obj, Vector3 position, Quaternion rotation, PoolType poolType = PoolType.None) where T : Object
        {
            if (!_objectPools.ContainsKey(obj))
            {
                _objectPools.Add(obj, new Pool(obj));
            }

            T spawnableObj = _objectPools[obj].GetPool(_objectResolver, position, rotation) as T;

            if (spawnableObj == null)
            {
                return null;
            }

            return spawnableObj;
        }

        public void ClearPool(bool includePersistent)
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