using UnityEngine;
using System;

namespace UIFramework
{
    public abstract class UIScreenController<TProps> : MonoBehaviour, IUIScreenController
        where TProps : IScreenProperties
    {
        [Header("Screen Animations")] 
        [Tooltip("Animation that shows the screen")] 
        [SerializeField]
        private TransitionComponent animIn;

        [Tooltip("Animation that hides the screen")] 
        [SerializeField]
        private TransitionComponent animOut;

        [Header("Screen properties")]
        [Tooltip(
            "This is the data payload and settings for this screen. You can rig this directly in a prefab and/or pass it when you show this screen")]
        [SerializeField]
        private TProps properties;

        public string ScreenId { get; set; }

        public TransitionComponent AnimIn
        {
            get { return animIn; }
            set { animIn = value; }
        }

        public TransitionComponent AnimOut
        {
            get { return animOut; }
            set { animOut = value; }
        }

        public Action<IUIScreenController> InTransitionFinished { get; set; }

        public Action<IUIScreenController> OutTransitionFinished { get; set; }

        public Action<IUIScreenController> CloseRequest { get; set; }

        public Action<IUIScreenController> ScreenDestroyed { get; set; }

        public bool IsVisible { get; private set; }

        protected TProps Properties
        {
            get { return properties; }
            set { properties = value; }
        }

        protected virtual void Awake()
        {
            AddListeners();
        }

        protected virtual void OnDestroy()
        {
            if (ScreenDestroyed != null)
            {
                ScreenDestroyed(this);
            }

            InTransitionFinished = null;
            OutTransitionFinished = null;
            CloseRequest = null;
            ScreenDestroyed = null;
            RemoveListeners();
        }

        protected virtual void AddListeners()
        {
        }

        protected virtual void RemoveListeners()
        {
        }

        protected virtual void OnPropertiesSet()
        {
        }

        protected virtual void WhileHiding()
        {
        }

        protected virtual void SetProperties(TProps props)
        {
            properties = props;
        }

        protected virtual void HierarchyFixOnShow()
        {
        }

        public void Hide(bool animate = true)
        {
            DoAnimation(animate ? animOut : null, OnTransitionOutFinished, false);
            WhileHiding();
        }

        public void Show(IScreenProperties props = null)
        {
            if (props != null)
            {
                if (props is TProps)
                {
                    SetProperties((TProps) props);
                }
                else
                {
                    Debug.LogError("Properties passed have wrong type! (" + props.GetType() + " instead of " +
                                   typeof(TProps) + ")");
                    return;
                }
            }

            HierarchyFixOnShow();
            OnPropertiesSet();

            if (!gameObject.activeSelf)
            {
                DoAnimation(animIn, OnTransitionInFinished, true);
            }
            else
            {
                if (InTransitionFinished != null)
                {
                    InTransitionFinished(this);
                }
            }
        }

        private void DoAnimation(TransitionComponent caller, Action callWhenFinished, bool isVisible)
        {
            if (caller == null)
            {
                gameObject.SetActive(isVisible);
                if (callWhenFinished != null)
                {
                    callWhenFinished();
                }
            }
            else
            {
                if (isVisible && !gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }

                caller.Animate(transform, callWhenFinished);
            }
        }

        private void OnTransitionInFinished()
        {
            IsVisible = true;

            if (InTransitionFinished != null)
            {
                InTransitionFinished(this);
            }
        }

        private void OnTransitionOutFinished()
        {
            IsVisible = false;
            gameObject.SetActive(false);

            if (OutTransitionFinished != null)
            {
                OutTransitionFinished(this);
            }
        }
    }
}
