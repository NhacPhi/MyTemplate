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
    private string currentWeapon = "";

    private void OnEnable()
    {
        UIEvent.OnSelectWeaponEnchanceFromCharacter += SetCurrentCharacterID;

        UIEvent.OnSlelectWeaponEnchance += SetCurrentWeapon;

        btnExit.onClick.AddListener(() =>
        {
            if (currentCharacter != "")
            {
                UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
            }

            if (currentWeapon != "")
            {
                UIEvent.OnSelectInventoryItem?.Invoke(currentWeapon);

                UIEvent.OnWeaponUpgraded?.Invoke(currentWeapon);
            }
            uiManager.CloseWindowScene(ScreenIds.UpgradeRelicScene);
        });
    }

    private void OnDisable()
    {
        UIEvent.OnSelectWeaponEnchanceFromCharacter -= SetCurrentCharacterID;
        UIEvent.OnSlelectWeaponEnchance -= SetCurrentWeapon;

        btnExit.onClick.RemoveAllListeners();
    }
    // Start is called before the first frame update
    void Start()
    {
        currencyMM.UpdateCurrency();
    }

    public void SetCurrentCharacterID(string uuid)
    {
        currentCharacter = uuid;
    }

    public void SetCurrentWeapon(string uuid)
    {
        currentWeapon = uuid;
    }
}
