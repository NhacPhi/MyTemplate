using System;

namespace UIFramework {
    public interface IUIScreenController {
        string ScreenId { get; set; }
        bool IsVisible { get; }

        void Show(IScreenProperties props = null);
        void Hide(bool animate = true);

        Action<IUIScreenController> InTransitionFinished { get; set; }
        Action<IUIScreenController> OutTransitionFinished { get; set; }
        Action<IUIScreenController> CloseRequest { get; set; }
        Action<IUIScreenController> ScreenDestroyed { get; set; }
    }




    public interface IWindowController : IUIScreenController {
        bool HideOnForegroundLost { get; }
        bool IsPopup { get; }
        WindowPriority WindowPriority { get; }
    }




    public interface IPanelController : IUIScreenController {
        PanelPriority Priority { get; }
    }
}
