#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEngine;

public class AttributeViewEditor : ViewEditor
{
    private const string AttributeTitle = "Attributes";

    private Color progressColor = Color.white;
    private Color backgroundColor = Color.black;

    public AttributeViewEditor(StatsController stats) : base(stats)
    {
        statsController.NotifyEditor -= Rebuild;
        statsController.NotifyEditor += Rebuild;
    }

    public AttributeViewEditor() : base()
    {

    }

    ~AttributeViewEditor()
    {
        if (statsController == null) return;
        statsController.NotifyEditor -= Rebuild;
    }

    protected override void SetTitle()
    {
        title = AttributeTitle;
    }

    protected override void Rebuild()
    {
        if (statsController == null || statsController.Attributes == null) return;

        Body.Clear();

        foreach(AttributeType key in statsController.Attributes.Keys)
        {
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;

            var attribute = statsController.GetAttribute(key);

            var curValue = attribute.Value;
            var maxValue = attribute.MaxValue;

            ProgressBar progressBar = new ProgressBar();
            progressBar.style.alignSelf = Align.Center;

            var progress = progressBar.Q(className: "unity-progress-bar__progress");
            var background = progressBar.Q(className: "unity-progress-bar__background");

            progress.style.backgroundColor = progressColor;
            background.style.backgroundColor = backgroundColor;

            progressBar.style.width = 100f;
            progressBar.style.height = 10f;
            progressBar.value = curValue;
            progressBar.highValue = maxValue;
            root.Add(progressBar);

            Label label = new Label($"{key} : {curValue} / {maxValue}");
            label.style.fontSize = 12;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(label);

            Body.Add(root); 
        }
    }
}
#endif