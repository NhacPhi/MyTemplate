using UnityEngine;

namespace Tech.Composite
{
    public class CoreComponent : MonoBehaviour
    {
        protected Core core;

        protected virtual void Awake()
        {
            core = GetComponent<Core>();

            if(!core)
            {
                core = GetComponent<Core>();
            }

            LoadComponent();
        }

        public virtual void LoadComponent()
        {

        }
    }
}

