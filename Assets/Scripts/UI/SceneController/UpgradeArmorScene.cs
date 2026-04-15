using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UpgradeArmorScene : WindowController
{
    [SerializeField] private Button btnExit;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;

    private string currentArmor = "";

    private void OnEnable()
    {
        UIEvent.OnSelectArmorUpgrade += SetCurrentArmor;

        btnExit.onClick.AddListener(() =>
        {
            UIEvent.OnCloseUpgradeArmorScene?.Invoke();

            uiManager.CloseWindowScene(ScreenIds.UpgradeArmorScene);

            UIEvent.OnSelectInventoryItem?.Invoke(currentArmor);

            UIEvent.OnArmorUpgraded?.Invoke(currentArmor);
        });
    }

    private void OnDisable()
    {
        UIEvent.OnSelectArmorUpgrade -= SetCurrentArmor;

        btnExit.onClick.RemoveAllListeners();
    }

    private void Start()
    {

        currencyMM.UpdateCurrency();
    }

    private void SetCurrentArmor(string id)
    {
        currentArmor= id;
    }

}
