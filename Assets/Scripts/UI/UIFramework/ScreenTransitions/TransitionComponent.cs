using UnityEngine;
using System;

namespace UIFramework {

    public abstract class TransitionComponent : MonoBehaviour {

        public abstract void Animate(Transform target, Action callWhenFinished);
    }
}
