using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private List<InteractionOption> interactions;

    private void OnEnable()
    {
        UIEvent.OnInterationUI += UpdateInteractionUI;
    }

    private void OnDisable()
    {
        UIEvent.OnInterationUI -= UpdateInteractionUI;
    }

    private void UpdateInteractionUI(bool value, InteractionType type)
    {
        var interaction = interactions.Find(interaction => interaction.Type == type);
        switch (type)
        { 
            case InteractionType.None:
                foreach (var obj in interactions)
                {
                    obj.gameObject.SetActive(false);
                }
                break;
            case InteractionType.Talk:
                interaction.gameObject.SetActive(value);
                break;
            case InteractionType.PickUp:
                interaction.gameObject.SetActive(value);
                break;
            case InteractionType.Cook:
                interaction.gameObject.SetActive(value);
                break;
        }


    }
}
