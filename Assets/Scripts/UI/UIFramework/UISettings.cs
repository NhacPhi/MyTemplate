using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using VContainer;
using VContainer.Unity;

namespace UIFramework
{




    
    [CreateAssetMenu(fileName = "UISettings", menuName = "UIFramework/UI Settings")]
    public class UISettings : ScriptableObject
    {
        [Tooltip("Prefab for the UI Frame structure itself")]
        [SerializeField] private UIFrame templateUIPrefab = null;
        [Tooltip("Prefabs for all the screens (both Panels and Windows) that are to be instanced and registered when the UI is instantiated")]
        [SerializeField] private List<GameObject> screensToRegister = null;
        [Tooltip("In case a screen prefab is not deactivated, should the system automatically deactivate its GameObject upon instantiation? If false, the screen will be at a visible state upon instantiation.")]
        [SerializeField] private bool deactivateScreenGOs = true;


        [Inject] private IObjectResolver _objectResolver;





        public UIFrame CreateUIInstance(bool instanceAndRegisterScreens = true) {
            var newUI = Instantiate(templateUIPrefab);

            if (instanceAndRegisterScreens) {
                foreach (var screen in screensToRegister) {
                    screen.SetActive(false);
                    var screenInstance = _objectResolver.Instantiate(screen); //Instantiate(screen);
                    //_objectResolver?.InjectGameObject(screenInstance);
                    var screenController = screenInstance.GetComponent<IUIScreenController>();

                    if (screenController != null) {
                        newUI.RegisterScreen(screen.name, screenController, screenInstance.transform);
                        if (deactivateScreenGOs && screenInstance.activeSelf) {
                            screenInstance.SetActive(false);
                        }
                    }
                    else {
                        Debug.LogError("[UIConfig] Screen doesn't contain a ScreenController! Skipping " + screen.name);
                    }
                }
            }

            return newUI;
        }
        
        private void OnValidate() {
            List<GameObject> objectsToRemove = new List<GameObject>();
            for(int i = 0; i < screensToRegister.Count; i++) {
                var screenCtl = screensToRegister[i].GetComponent<IUIScreenController>();
                if (screenCtl == null) {
                    objectsToRemove.Add(screensToRegister[i]);
                }
            }

            if (objectsToRemove.Count > 0) {
                Debug.LogError("[UISettings] Some GameObjects that were added to the Screen Prefab List didn't have ScreenControllers attached to them! Removing.");
                foreach (var obj in objectsToRemove) {
                    Debug.LogError("[UISettings] Removed " + obj.name + " from " + name + " as it has no Screen Controller attached!");
                    screensToRegister.Remove(obj);
                }
            }
        }        
    }
}
