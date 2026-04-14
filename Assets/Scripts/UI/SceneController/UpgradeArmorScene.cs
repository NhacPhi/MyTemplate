using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class UpgradeArmorScene : WindowController
{
    [SerializeField] private Button btnExit;

    [Inject] private UIManager uiManager;
    [Inject] private CurrencyManager currencyMM;

    private void Start()
    {
        btnExit.onClick.AddListener(() =>
        {
            //if (currentCharacter != "")
            //{
            //    UIEvent.OnSelectCharacterAvatar?.Invoke(currentCharacter);
            //}
            uiManager.CloseWindowScene();
        });
        currencyMM.UpdateCurrency();
    }
}
