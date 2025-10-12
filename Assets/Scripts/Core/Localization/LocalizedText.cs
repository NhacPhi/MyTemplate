using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    private string _stringID;

    private void Awake()
    {
        _stringID = gameObject.name;
        UIEvent.OnLanguageChanged += UpdateText;
    }
    private void OnDestroy()
    {
        UIEvent.OnLanguageChanged -= UpdateText;
    }

    private void Start()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        var textMesh = GetComponent<TextMeshProUGUI>();
        textMesh.text = LocalizationManager.Instance.GetLocalizedValue(_stringID);
    }
}
