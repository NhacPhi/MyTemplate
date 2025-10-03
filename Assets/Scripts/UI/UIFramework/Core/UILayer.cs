using UnityEngine;
using System.Collections.Generic;

namespace UIFramework {

    public abstract class UILayer<TScreen> : MonoBehaviour where TScreen : IUIScreenController {
        protected Dictionary<string, TScreen> registeredScreens;

        public abstract void ShowScreen(TScreen screen);

        public abstract void ShowScreen<TProps>(TScreen screen, TProps properties) where TProps : IScreenProperties;

        public abstract void HideScreen(TScreen screen);

        public virtual void Initialize() {
            registeredScreens = new Dictionary<string, TScreen>();
        }

        public virtual void ReparentScreen(IUIScreenController controller, Transform screenTransform) {
            screenTransform.SetParent(transform, false);
        }

        public void RegisterScreen(string screenId, TScreen controller) {
            if (!registeredScreens.ContainsKey(screenId)) {
                ProcessScreenRegister(screenId, controller);
            }
            else {
                Debug.LogError("[AUILayerController] Screen controller already registered for id: " + screenId);
            }
        }

        public void UnregisterScreen(string screenId, TScreen controller) {
            if (registeredScreens.ContainsKey(screenId)) {
                ProcessScreenUnregister(screenId, controller);
            }
            else {
                Debug.LogError("[AUILayerController] Screen controller not registered for id: " + screenId);
            }
        }

        public void ShowScreenById(string screenId) {
            TScreen ctl;
            if (registeredScreens.TryGetValue(screenId, out ctl)) {
                ShowScreen(ctl);
            }
            else {
                Debug.LogError("[AUILayerController] Screen ID " + screenId + " not registered to this layer!");
            }
        }

        public void ShowScreenById<TProps>(string screenId, TProps properties) where TProps : IScreenProperties {
            TScreen ctl;
            if (registeredScreens.TryGetValue(screenId, out ctl)) {
                ShowScreen(ctl, properties);
            }
            else {
                Debug.LogError("[AUILayerController] Screen ID " + screenId + " not registered!");
            }
        }

        public void HideScreenById(string screenId) {
            TScreen ctl;
            if (registeredScreens.TryGetValue(screenId, out ctl)) {
                HideScreen(ctl);
            }
            else {
                Debug.LogError("[AUILayerController] Could not hide Screen ID " + screenId + " as it is not registered to this layer!");
            }
        }

        public bool IsScreenRegistered(string screenId) {
            return registeredScreens.ContainsKey(screenId);
        }

        public virtual void HideAll(bool shouldAnimateWhenHiding = true) {
            foreach (var screen in registeredScreens) {
                screen.Value.Hide(shouldAnimateWhenHiding);
            }
        }

        protected virtual void ProcessScreenRegister(string screenId, TScreen controller) {
            controller.ScreenId = screenId;
            registeredScreens.Add(screenId, controller);
            controller.ScreenDestroyed += OnScreenDestroyed;
        }

        protected virtual void ProcessScreenUnregister(string screenId, TScreen controller) {
            controller.ScreenDestroyed -= OnScreenDestroyed;
            registeredScreens.Remove(screenId);
        }

        private void OnScreenDestroyed(IUIScreenController screen) {
            if (!string.IsNullOrEmpty(screen.ScreenId)
                && registeredScreens.ContainsKey(screen.ScreenId)) {
                UnregisterScreen(screen.ScreenId, (TScreen) screen);
            }
        }
    }
}
