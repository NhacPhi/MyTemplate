using UnityEngine;
using UnityEngine.UI;

public class SkillCharacterUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _icon;
    
    [Header("Enhancement Indicators")]
    [Tooltip("Indicators for enhancement level 1 and 2")]
    [SerializeField] private GameObject[] _activeRadios;   // Active state indicators
    [SerializeField] private GameObject[] _inactiveRadios; // Inactive state indicators (optional)

    /// <summary>
    /// Sets the skill icon.
    /// </summary>
    public void SetIcon(Sprite icon)
    {
        if (_icon != null)
        {
            _icon.sprite = icon;
        }
    }

    /// <summary>
    /// Updates the enhancement UI based on level.
    /// enhancementLevel: 0 (none), 1 (level 1), 2 (level 2)
    /// </summary>
    public void UpdateEnhancement(int enhancementLevel)
    {
        for (int i = 0; i < 2; i++)
        {
            bool isEnhanced = i < enhancementLevel;

            // Update active indicators
            if (_activeRadios != null && i < _activeRadios.Length && _activeRadios[i] != null)
            {
                _activeRadios[i].SetActive(isEnhanced);
            }

            // Update inactive indicators (if any)
            if (_inactiveRadios != null && i < _inactiveRadios.Length && _inactiveRadios[i] != null)
            {
                _inactiveRadios[i].SetActive(!isEnhanced);
            }
        }
    }

    /// <summary>
    /// Helper method to set both icon and enhancement level.
    /// </summary>
    public void SetSkillUI(Sprite icon, int enhancementLevel)
    {
        SetIcon(icon);
        UpdateEnhancement(enhancementLevel);
    }
}
