using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class InteractionUI : MonoBehaviour
{
    [Inject] private GameDataBase gameDataBase;

    [SerializeField] private List<InteractionOption> interactions;

    private void Start()
    {
        UpdateInteractionUI(null);
    }

    private void OnEnable()
    {
        UIEvent.OnUpdateInteractionsUI += UpdateInteractionUI;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UIEvent.OnUpdateInteractionsUI -= UpdateInteractionUI;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        UpdateInteractionUI(null);
    }

    private void UpdateInteractionUI(List<Interaction> activeInteractions)
    {
        // Tắt hết các nút hiện tại
        foreach (var o in interactions)
        {
            o.gameObject.SetActive(false);
        }

        if (activeInteractions == null || activeInteractions.Count == 0) return;

        foreach (var interaction in activeInteractions)
        {
            if (interaction == null || interaction.interactableObject == null) continue;

            // Tìm nút có DefaultType tương ứng với loại interaction
            var optionUI = interactions.Find(o => o.DefaultType == interaction.type && !o.gameObject.activeSelf);
            
            if (optionUI == null)
            {
                Debug.LogWarning($"[InteractionUI] Không tìm thấy nút InteractionOption nào cho type {interaction.type} hoặc đã dùng hết nút loại này!");
                continue;
            }

            optionUI.Setup(interaction);
            optionUI.gameObject.SetActive(true);

            // Tùy chỉnh icon và text dựa vào loại tương tác (nếu cần bổ sung thêm)
            switch (interaction.type)
            {
                case InteractionType.Talk:
                    string talkText = "";
                    StepController stepController = interaction.interactableObject.GetComponent<StepController>();
                    if (stepController != null && stepController.Actor != null)
                    {
                        string locKey = "STR_" + stepController.Actor.ID.ToUpper();
                        talkText = LocalizationManager.Instance.GetLocalizedValue(locKey);
                        if (string.IsNullOrEmpty(talkText)) 
                        {
                            talkText = stepController.Actor.ID; // Fallback nếu không tìm thấy key
                        }
                    }
                    optionUI.SetContentText(talkText);
                    break;
                case InteractionType.PickUp:
                    ItemPickup itemPickup = interaction.interactableObject.GetComponent<ItemPickup>();
                    if (itemPickup != null && gameDataBase != null)
                    {
                        var config = gameDataBase.GetItemConfig(itemPickup.itemID);
                        if (config != null)
                        {
                            if (config.Icon != null)
                            {
                                optionUI.SetIcon(config.Icon);
                            }
                            string itemName = LocalizationManager.Instance.GetLocalizedValue(config.Name);
                            optionUI.SetContentText(itemName);
                        }
                    }
                    break;
                case InteractionType.Cook:
                    //optionUI.SetContentText(LocalizationManager.Instance.GetLocalizedValue("cook_action") ?? "Cook");
                    break;
                case InteractionType.Fighting:
                    break;
            }
        }
    }
}
