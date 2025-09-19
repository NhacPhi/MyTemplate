using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Tech.Composite
{
    public class Core : MonoBehaviour
    {
        private List<CoreComponent> _coreCompoenents;

        private void Awake()
        {
            
        }

        private void Initialize()
        {
            if (_coreCompoenents != null) return;

            _coreCompoenents = GetComponentsInChildren<CoreComponent>().ToList();
            LoadComponent();
        }

        protected virtual void LoadComponent()
        {

        }

        public void AddCoreComponent(CoreComponent component)
        {
            if(_coreCompoenents.Contains(component)) return;
            _coreCompoenents.Add(component);
        }

        public T GetCoreComponent<T>()
        {
            Initialize();
            foreach(var component in _coreCompoenents)
            {
                if (component is T TComponent) return TComponent;
            }

            return default;
        }
        
        public T GetComponent<T>(string objName) where T : CoreComponent
        {
            Initialize();
            foreach (var component in _coreCompoenents)
            {
                if(component is T tmpComponent && tmpComponent.gameObject.name == objName)
                {
                    return tmpComponent;
                }
            }

            return null;
        }

        public List<T> GetCoreComponents<T>()
        {
            Initialize();
            return _coreCompoenents.OfType<T>().ToList();
        }

        public void RemoveComponent<T>() where T : CoreComponent
        {
            var componentList = _coreCompoenents.OfType<T>().ToList();

            foreach(T component in componentList)
            {
                Destroy(component);
            }

            _coreCompoenents.RemoveAll(x => x == null);
        }
    }

}
