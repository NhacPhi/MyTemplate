using UnityEngine;

namespace Tech.Singleton
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instace;

        public static T Instance
        {
            get
            {
                if (_instace) return _instace;

                _instace = FindObjectOfType<T>();
                if (_instace) return _instace;

                _instace = new GameObject(typeof(T).Name).AddComponent<T>();
                return null;
            }
        }

        protected virtual void Awake()
        {
            if(_instace != null && _instace != this)
            {
                Destroy(gameObject);
                return;
            }

            _instace = this as T;
        }
    }

    public class SingletonPersistent<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }

}
