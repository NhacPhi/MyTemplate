using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;

public class ToggleBase : MonoBehaviour
{
    protected Toggle toggle;
    // Start is called before the first frame update
    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnSelected);
    }

    public virtual void OnSelected(bool isOn)
    {

    }
}
