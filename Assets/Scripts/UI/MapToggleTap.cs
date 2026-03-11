using UnityEngine.UI;
using UnityEngine;

public class MapToggleTap : ToggleBase
{
    [SerializeField] private GameSceneSO location;

    [SerializeField] private Sprite spriteLocation;


    public override void OnSelected(bool isOn)
    {
        if (isOn)
        {
            UIEvent.OnSelectToggleMap?.Invoke(location, spriteLocation);
        }
    }
}
