using UnityEngine;

namespace UIFramework {

    [System.Serializable] 
    public class WindowProperties : IWindowProperties {
        [SerializeField] 
        protected bool hideOnForegroundLost = true;

        [SerializeField] 
        protected WindowPriority windowQueuePriority = WindowPriority.ForceForeground;

        [SerializeField]
        protected bool isPopup = false;

        public WindowProperties() {
            hideOnForegroundLost = true;
            windowQueuePriority = WindowPriority.ForceForeground;
            isPopup = false;
        }







        public WindowPriority WindowQueuePriority {
            get { return windowQueuePriority; }
            set { windowQueuePriority = value; }
        }





        public bool HideOnForegroundLost {
            get { return hideOnForegroundLost; }
            set { hideOnForegroundLost = value; }
        }






        public bool SuppressPrefabProperties { get; set; }






        public bool IsPopup {
            get { return isPopup; }
            set { isPopup = value; }
        }

        public WindowProperties(bool suppressPrefabProperties = false) {
            WindowQueuePriority = WindowPriority.ForceForeground;
            HideOnForegroundLost = false;
            SuppressPrefabProperties = suppressPrefabProperties;
        }

        public WindowProperties(WindowPriority priority, bool hideOnForegroundLost = false, bool suppressPrefabProperties = false) {
            WindowQueuePriority = priority;
            HideOnForegroundLost = hideOnForegroundLost;
            SuppressPrefabProperties = suppressPrefabProperties;
        }
    }
}
