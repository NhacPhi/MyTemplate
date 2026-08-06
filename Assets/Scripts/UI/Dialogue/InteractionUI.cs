using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class InteractionUI : MonoBehaviour
{
    [Inject] private GameDataBase gameDataBase;
    private GameNarrativeData gameNarrativeData;

    [SerializeField] private List<InteractionOption> interactions;

    private bool isDialogueActive = false;

    private void Start()
    {
        UpdateInteractionUI(null);
    }

    private void OnEnable()
    {
        isDialogueActive = false;
        UIEvent.OnUpdateInteractionsUI += UpdateInteractionUI;
        GameEvent.OnStartDialogue += HandleStartDialogue;
        GameEvent.OnOpenDialogue += HandleOpenDialogue;
        GameEvent.OnEndDialogue += HandleEndDialogue;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        UIEvent.OnRequestRefreshInteractionsUI?.Invoke();
    }

    private void OnDisable()
    {
        UIEvent.OnUpdateInteractionsUI -= UpdateInteractionUI;
        GameEvent.OnStartDialogue -= HandleStartDialogue;
        GameEvent.OnOpenDialogue -= HandleOpenDialogue;
        GameEvent.OnEndDialogue -= HandleEndDialogue;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
        UpdateInteractionUI(null);
    }

    private void HandleStartDialogue(DialogueConfig config)
    {
        isDialogueActive = true;
        UpdateInteractionUI(null);
    }

    private void HandleOpenDialogue(string dialogueLine, ActorConfig actor)
    {
        isDialogueActive = true;
        UpdateInteractionUI(null);
    }

    private void HandleEndDialogue(DialogueType type)
    {
        isDialogueActive = false;
        UIEvent.OnRequestRefreshInteractionsUI?.Invoke();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        UpdateInteractionUI(null);
    }

    private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        UpdateInteractionUI(null);
    }

    private GameNarrativeData GetGameNarrativeData()
    {
        if (gameNarrativeData != null) return gameNarrativeData;

        if (GameplayScope.Instance != null && GameplayScope.Instance.Container != null)
        {
            gameNarrativeData = GameplayScope.Instance.Container.Resolve<GameNarrativeData>();
        }
        else
        {
            gameNarrativeData = FindObjectOfType<GameNarrativeData>();
        }
        return gameNarrativeData;
    }

    private void UpdateInteractionUI(List<Interaction> activeInteractions)
    {
        // Tắt hết các nút hiện tại
        foreach (var o in interactions)
        {
            o.gameObject.SetActive(false);
        }

        if (isDialogueActive) return;

        if (activeInteractions == null || activeInteractions.Count == 0) return;

        foreach (var interaction in activeInteractions)
        {
            if (interaction == null || interaction.interactableObject == null) continue;

            // Tìm nút có DefaultType tương ứng với loại interaction
            var optionUI = interactions.Find(o => o.DefaultType == interaction.type && !o.gameObject.activeSelf);
            
            if (optionUI == null)
            {
                Debug.LogWarning($"[InteractionUI] Thất bại! Không tìm thấy nút InteractionOption nào trong danh sách `interactions` có DefaultType = {interaction.type}!");
                continue;
            }

            optionUI.Setup(interaction);
            optionUI.gameObject.SetActive(true);
            Debug.Log($"[InteractionUI] Đã bật nút InteractionOption thành công cho type: {interaction.type}");

            // Tùy chỉnh icon và text dựa vào loại tương tác (nếu cần bổ sung thêm)
            switch (interaction.type)
            {
                case InteractionType.Talk:
                    string talkText = "";
                    StepController stepController = interaction.interactableObject.GetComponent<StepController>()
                        ?? interaction.interactableObject.GetComponentInParent<StepController>();
                    if (stepController != null && stepController.Actor != null)
                    {
                        var narrativeData = GetGameNarrativeData();
                        ActorConfig actorConfig = narrativeData != null ? narrativeData.GetActorConfig(stepController.Actor.ID) : null;
                        if (actorConfig != null && actorConfig.Name != 0)
                        {
                            talkText = LocalizationManager.Instance.GetLocalizedValue(actorConfig.Name);
                        }

                        if (string.IsNullOrEmpty(talkText) || talkText == "Localized text not found") 
                        {
                            string locKey = "STR_" + stepController.Actor.ID.ToUpper();
                            talkText = LocalizationManager.Instance.GetLocalizedValue(locKey);
                        }

                        if (string.IsNullOrEmpty(talkText) || talkText == "Localized text not found") 
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
                case InteractionType.Flying:
                    break;
            }
        }
    }
}
