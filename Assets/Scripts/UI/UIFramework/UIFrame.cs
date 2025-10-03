using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{

    public class UIFrame : MonoBehaviour
    {
        [Tooltip("Set this to false if you want to manually initialize this UI Frame.")]
        [SerializeField] private bool initializeOnAwake = true;
        
        private PanelUILayer panelLayer;
        private WindowUILayer windowLayer;

        private Canvas mainCanvas;
        private GraphicRaycaster graphicRaycaster;

        public Canvas MainCanvas {
            get {
                if (mainCanvas == null) {
                    mainCanvas = GetComponent<Canvas>();
                }

                return mainCanvas;
            }
        }

        public Camera UICamera {
            get { return MainCanvas.worldCamera; }
        }

        private void Awake() {
            if (initializeOnAwake) {
                Initialize();    
            }
        }


        public virtual void Initialize() {
            if (panelLayer == null) {
                panelLayer = gameObject.GetComponentInChildren<PanelUILayer>(true);
                if (panelLayer == null) {
                    Debug.LogError("[UI Frame] UI Frame lacks Panel Layer!");
                }
                else {
                    panelLayer.Initialize();
                }
            }

            if (windowLayer == null) {
                windowLayer = gameObject.GetComponentInChildren<WindowUILayer>(true);
                if (panelLayer == null) {
                    Debug.LogError("[UI Frame] UI Frame lacks Window Layer!");
                }
                else {
                    windowLayer.Initialize();
                    windowLayer.RequestScreenBlock += OnRequestScreenBlock;
                    windowLayer.RequestScreenUnblock += OnRequestScreenUnblock;
                }
            }

            graphicRaycaster = MainCanvas.GetComponent<GraphicRaycaster>();
        }

        public void ShowPanel(string screenId) {
            panelLayer.ShowScreenById(screenId);
        }

        public void ShowPanel<T>(string screenId, T properties) where T : IPanelProperties {
            panelLayer.ShowScreenById<T>(screenId, properties);
        }

        public void HidePanel(string screenId) {
            panelLayer.HideScreenById(screenId);
        }

        public void OpenWindow(string screenId) {
            windowLayer.ShowScreenById(screenId);
        }

        public void CloseWindow(string screenId) {
            windowLayer.HideScreenById(screenId);
        }

        public void CloseCurrentWindow() {
            if (windowLayer.CurrentWindow != null) {
                CloseWindow(windowLayer.CurrentWindow.ScreenId);    
            }
        }

        public void OpenWindow<T>(string screenId, T properties) where T : IWindowProperties {
            windowLayer.ShowScreenById<T>(screenId, properties);
        }

        public void ShowScreen(string screenId) {
            Type type;
            if (IsScreenRegistered(screenId, out type)) {
                if (type == typeof(IWindowController)) {
                    OpenWindow(screenId);
                }
                else if (type == typeof(IPanelController)) {
                    ShowPanel(screenId);
                }
            }
            else {
                Debug.LogError(string.Format("Tried to open Screen id {0} but it's not registered as Window or Panel!",
                    screenId));
            }
        }

        public void RegisterScreen(string screenId, IUIScreenController controller, Transform screenTransform) {
            IWindowController window = controller as IWindowController;
            if (window != null) {
                windowLayer.RegisterScreen(screenId, window);
                if (screenTransform != null) {
                    windowLayer.ReparentScreen(controller, screenTransform);
                }

                return;
            }

            IPanelController panel = controller as IPanelController;
            if (panel != null) {
                panelLayer.RegisterScreen(screenId, panel);
                if (screenTransform != null) {
                    panelLayer.ReparentScreen(controller, screenTransform);
                }
            }
        }

        public void RegisterPanel<TPanel>(string screenId, TPanel controller) where TPanel : IPanelController {
            panelLayer.RegisterScreen(screenId, controller);
        }

        public void UnregisterPanel<TPanel>(string screenId, TPanel controller) where TPanel : IPanelController {
            panelLayer.UnregisterScreen(screenId, controller);
        }

        public void RegisterWindow<TWindow>(string screenId, TWindow controller) where TWindow : IWindowController {
            windowLayer.RegisterScreen(screenId, controller);
        }

        public void UnregisterWindow<TWindow>(string screenId, TWindow controller) where TWindow : IWindowController {
            windowLayer.UnregisterScreen(screenId, controller);
        }

        public bool IsPanelOpen(string panelId) {
            return panelLayer.IsPanelVisible(panelId);
        }

        public void HideAll(bool animate = true) {
            CloseAllWindows(animate);
            HideAllPanels(animate);
        }

        public void HideAllPanels(bool animate = true) {
            panelLayer.HideAll(animate);
        }

        public void CloseAllWindows(bool animate = true) {
            windowLayer.HideAll(animate);
        }

        public bool IsScreenRegistered(string screenId) {
            if (windowLayer.IsScreenRegistered(screenId)) {
                return true;
            }

            if (panelLayer.IsScreenRegistered(screenId)) {
                return true;
            }

            return false;
        }

        public bool IsScreenRegistered(string screenId, out Type type) {
            if (windowLayer.IsScreenRegistered(screenId)) {
                type = typeof(IWindowController);
                return true;
            }

            if (panelLayer.IsScreenRegistered(screenId)) {
                type = typeof(IPanelController);
                return true;
            }

            type = null;
            return false;
        }

        private void OnRequestScreenBlock() {
            if (graphicRaycaster != null) {
                graphicRaycaster.enabled = false;
            }
        }

        private void OnRequestScreenUnblock() {
            if (graphicRaycaster != null) {
                graphicRaycaster.enabled = true;
            }
        }

        public IWindowController GetCurrentWindow()
        {
            return windowLayer.CurrentWindow;
        }    
    }
}
