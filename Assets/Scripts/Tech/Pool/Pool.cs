using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System;
using VContainer;
using VContainer.Unity;
namespace Tech.Pool
{
    [Serializable]
    public class Pool
    {
        private Stack<Object> _inActiveObject = new();
        private Object _baseObject;

        public Pool(Object obj)
        {
            _baseObject = obj;
        }

        public Object GetPool(IObjectResolver objectResolver, Vector3 position = default, Quaternion rotaion = default)
        {
            GameObject go = null;

            if(_inActiveObject.Count > 0)
            {
                var tmp = _inActiveObject.Pop();
                go = GetInstnace(tmp);
                go.transform.position = position;
                go.transform.rotation = rotaion;
                go.SetActive(true);

                return tmp;
            }

            if(_baseObject is GameObject gameObject)
            {
                go = objectResolver.Instantiate(gameObject, position, rotaion);
                go.AddComponent<ReturnToPool>().PoolObjects = this;
                return go;
            }

            if (_baseObject is not Component component) return null;

            var cloneComponent = objectResolver.Instantiate(component, position, rotaion);
            var returnToPool = cloneComponent.gameObject.AddComponent<ReturnToPool>();
            returnToPool.PoolObjects = this;
            returnToPool.RootCompoment = cloneComponent;
            return cloneComponent;

        }
        public void AddToPool(Object obj)
        {
            _inActiveObject.Push(obj);
        }

        private GameObject GetInstnace(Object obj)
        {
            if(obj is Component component)
            {
                return component.gameObject;
            }

            return obj as GameObject;
        }
    }

}
