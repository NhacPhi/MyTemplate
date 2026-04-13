using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
public class UpgradeRelicScene : WindowController
{
    [SerializeField] private Button btnExit;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;

    private string currentCharacter = "";

    private void OnEnable()
    {
        UIEvent.OnSelectWeaponEnchanceFromCharacter += SetCurrentCharacterID;
    }

    private void OnDisable()
    {
        UIEvent.OnSelectWeaponEnchanceFromCharacter -= SetCurrentCharacterID;
    }
    // Start is called before the first frame update
    void Start()
    {
        btnExit.onClick.AddListener(() =>
        {
            if (currentCharacter != "")
            {
                UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
            }
            uiManager.CloseWindowScene();
        });
        currencyMM.UpdateCurrency();
    }

    public void SetCurrentCharacterID(string uuid)
    {
        currentCharacter = uuid;
    }

}
