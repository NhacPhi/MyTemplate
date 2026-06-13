using UnityEngine.UI;
using UnityEngine;

public class ToggleBase : MonoBehaviour
{
    protected Toggle toggle;

    public Toggle Toggle
    {
        get
        {
            if (toggle == null) toggle = GetComponent<Toggle>();
            return toggle;
        }
    }

    protected virtual void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    protected virtual void Start()
    {
        if (Toggle != null)
        {
            Toggle.onValueChanged.AddListener(OnSelected);
            OnSelected(Toggle.isOn); // Initialize visual state
        }
    }

    public virtual void OnSelected(bool isOn)
    {
        transform.localScale = isOn ? new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;
    }

    public void ActiveToggle(bool value)
    {
        if (Toggle != null) Toggle.isOn = value;
    }
}
