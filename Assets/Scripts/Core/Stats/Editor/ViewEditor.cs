using UnityEngine.UIElements;
using UnityEngine;

public class ViewEditor : VisualElement
{
    protected string title = "";
    protected VisualElement head;
    public VisualElement Body;
    protected StatsController statsController;

    public ViewEditor(StatsController stats)
    {
        statsController = stats;
        InitView();
        Rebuild();
    }

    public ViewEditor()
    {
        InitView();
    }

    protected virtual void InitView()
    {
        SetTitle();

        head = new VisualElement();

        Body = new VisualElement();
        Body.style.paddingTop = 3;
        
        Label headerTitle = new Label(title);
        headerTitle.style.fontSize = 12;
        headerTitle.style.unityFontStyleAndWeight = FontStyle.Bold;

        head.Add(headerTitle);

        this.style.paddingTop = 5;
        this.style.paddingBottom = 5;
        this.Add(head);
        this.Add(Body);
    }

    protected virtual void SetTitle()
    {

    }

    protected virtual void Rebuild()
    {

    }
}
