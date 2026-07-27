using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

public enum InteractionType { None = 0, PickUp, Cook, Talk, Fighting, Flying };

public class InteractionManager : IStartable, IDisposable
{
    private readonly List<Interaction> potentialInteractions = new List<Interaction>();

    public void Start()
    {
        GameEvent.OnTriggerZoneChanged += OnTriggerChangeDetected;
        GameEvent.OnInteraction += OnInteractionButtonPress;
        GameEvent.OnExecuteSpecificInteraction += ExecuteInteraction;
        GameEvent.OnPlayerTransform += ClearAllInteractions;
        GameEvent.OnEndDialogue += OnDialogueEnd;
        UIEvent.OnRequestRefreshInteractionsUI += RequestUpdateUI;
    }

    public void Dispose()
    {
        GameEvent.OnTriggerZoneChanged -= OnTriggerChangeDetected;
        GameEvent.OnInteraction -= OnInteractionButtonPress;
        GameEvent.OnExecuteSpecificInteraction -= ExecuteInteraction;
        GameEvent.OnPlayerTransform -= ClearAllInteractions;
        GameEvent.OnEndDialogue -= OnDialogueEnd;
        UIEvent.OnRequestRefreshInteractionsUI -= RequestUpdateUI;
        ClearAllInteractions();
    }

    private void OnDialogueEnd(DialogueType type)
    {
        RequestUpdateUI();
    }

    public void OnTriggerChangeDetected(bool entered, GameObject obj)
    {
        if (entered)
            AddPotentialInteraction(obj);
        else
            RemovePotentialInteraction(obj);
    }

    private void AddPotentialInteraction(GameObject obj)
    {
        if (obj == null) return;

        // Tránh thêm trùng lặp cùng một object vào danh sách tương tác
        if (potentialInteractions.Exists(i => i.interactableObject == obj))
        {
            return;
        }

        Interaction newPotentialInteraction = new Interaction(InteractionType.None, obj);

        if (obj.CompareTag("Pickable"))
        {
            newPotentialInteraction.type = InteractionType.PickUp;
        }
        else if (obj.CompareTag("CookingPot"))
        {
            newPotentialInteraction.type = InteractionType.Cook;
        }
        else if (obj.CompareTag("NPC"))
        {
            newPotentialInteraction.type = InteractionType.Talk;
        }
        else if (obj.CompareTag("Fighting") || (obj.transform.parent != null && obj.transform.parent.CompareTag("Fighting")) || obj.GetComponent<BattleTrigger>() != null || obj.GetComponentInParent<BattleTrigger>() != null)
        {
            newPotentialInteraction.type = InteractionType.Fighting;
        }
        else if (obj.CompareTag("Flying") || (obj.transform.parent != null && obj.transform.parent.CompareTag("Flying")) || obj.GetComponent<LightStreakTeleporter>() != null || obj.GetComponentInParent<LightStreakTeleporter>() != null)
        {
            newPotentialInteraction.type = InteractionType.Flying;
            Debug.Log($"[InteractionManager] Phát hiện tương tác Flying thành công từ object: {obj.name}");
        }

        if (newPotentialInteraction.type != InteractionType.None)
        {
            potentialInteractions.Add(newPotentialInteraction);
            RequestUpdateUI();
        }
        else
        {
            Debug.LogWarning($"[InteractionManager] Đã chạm vào '{obj.name}' (Tag: '{obj.tag}'), nhưng không khớp loại InteractionType nào!");
        }
    }

    private void RemovePotentialInteraction(GameObject obj)
    {
        int count = potentialInteractions.RemoveAll(o => o.interactableObject == obj);
        if (count > 0)
        {
            RequestUpdateUI();
        }
    }

    private void RequestUpdateUI()
    {
        // Loại bỏ các object đã bị Destroy (== null theo Unity operator)
        potentialInteractions.RemoveAll(i => i.interactableObject == null);
        UIEvent.OnUpdateInteractionsUI?.Invoke(potentialInteractions);
    }

    private void ClearAllInteractions()
    {
        potentialInteractions.Clear();
        RequestUpdateUI();
    }

    public void ExecuteInteraction(Interaction interaction)
    {
        if (interaction == null || interaction.interactableObject == null) return;

        switch (interaction.type)
        {
            case InteractionType.Talk:
                StepController stepController = interaction.interactableObject.GetComponent<StepController>()
                    ?? interaction.interactableObject.GetComponentInParent<StepController>();
                if (stepController != null)
                {
                    stepController.InteractWithCharacter();
                }
                RequestUpdateUI();
                break;
            case InteractionType.Fighting:
                BattleTrigger battleTrigger = interaction.interactableObject.GetComponent<BattleTrigger>() 
                    ?? interaction.interactableObject.GetComponentInParent<BattleTrigger>();
                if (battleTrigger != null)
                {
                    battleTrigger.OpenPrepareScene();
                }
                else
                {
                    Debug.LogWarning($"[InteractionManager] Đối tượng '{interaction.interactableObject.name}' được nhận diện Fighting nhưng không tìm thấy component BattleTrigger!");
                }
                ClearAllInteractions();
                break;
            case InteractionType.PickUp:
                ItemPickup item = interaction.interactableObject.GetComponent<ItemPickup>();
                if (item != null)
                {
                    item.PickUp();
                    potentialInteractions.RemoveAll(i => i.interactableObject == interaction.interactableObject);
                    RequestUpdateUI();
                }
                else
                {
                    Debug.LogWarning("[InteractionManager] Đối tượng có tag Pickable nhưng không có script ItemPickup!");
                }
                break;
            case InteractionType.Cook:
                potentialInteractions.Remove(interaction);
                RequestUpdateUI();
                break;
            case InteractionType.Flying:
                LightStreakTeleporter teleporter = interaction.interactableObject.GetComponent<LightStreakTeleporter>() 
                    ?? interaction.interactableObject.GetComponentInParent<LightStreakTeleporter>();
                if (teleporter != null)
                {
                    teleporter.ExecuteTeleport();
                }
                ClearAllInteractions();
                break;
        }
    }

    private void OnInteractionButtonPress()
    {
        if (potentialInteractions.Count == 0)
            return;

        ExecuteInteraction(potentialInteractions[0]);
    }
}
