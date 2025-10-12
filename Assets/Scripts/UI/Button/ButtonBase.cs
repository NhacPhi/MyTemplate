using UnityEngine.UI;
using UnityEngine;
using System;

[Serializable]
public class ButtonBase : MonoBehaviour
{
    public Button Btn { get; private set; }

    private void Awake()
    {
        Btn = GetComponent<Button>();
        OnAwake();
    }

    public virtual void OnAwake()
    {

    }
}
