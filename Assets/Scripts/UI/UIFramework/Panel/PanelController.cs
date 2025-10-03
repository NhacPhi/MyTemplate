namespace UIFramework {

    public abstract class PanelController : PanelController<PanelProperties> { }

    public abstract class PanelController<T> : UIScreenController<T>, IPanelController where T : IPanelProperties {
        public PanelPriority Priority {
            get {
                if (Properties != null) {
                    return Properties.Priority;
                }
                else {
                    return PanelPriority.None;
                }
            }
        }

        protected sealed override void SetProperties(T props) {
            base.SetProperties(props);
        }
    }
}
