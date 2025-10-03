using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UIFramework {

    public class PanelUILayer : UILayer<IPanelController> {
        [SerializeField]
        [Tooltip("Settings for the priority para-layers. A Panel registered to this layer will be reparented to a different para-layer object depending on its Priority.")]
        private PanelPriorityLayerList priorityLayers = null;

        public override void ReparentScreen(IUIScreenController controller, Transform screenTransform) {
            var ctl = controller as IPanelController;
            if (ctl != null) {
                ReparentToParaLayer(ctl.Priority, screenTransform);
            }
            else {
                base.ReparentScreen(controller, screenTransform);
            }
        }

        public override void ShowScreen(IPanelController screen) {
            screen.Show();
        }

        public override void ShowScreen<TProps>(IPanelController screen, TProps properties) {
            screen.Show(properties);
        }

        public override void HideScreen(IPanelController screen) {
            screen.Hide();
        }

        public bool IsPanelVisible(string panelId) {
            IPanelController panel;
            if (registeredScreens.TryGetValue(panelId, out panel)) {
                return panel.IsVisible;
            }

            return false;
        }
        
        private void ReparentToParaLayer(PanelPriority priority, Transform screenTransform) {
            Transform trans;
            if (!priorityLayers.ParaLayerLookup.TryGetValue(priority, out trans)) {
                trans = transform;
            }
            
            screenTransform.SetParent(trans, false);
        }
    }
}
