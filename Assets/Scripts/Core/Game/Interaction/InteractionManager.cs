using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public enum InteractionType { None = 0, PickUp, Cook, Talk };
public class InteractionManager : MonoBehaviour
{
    //To store the objects we the player could potentially interact with
    private List<Interaction> potentialInteractions = new List<Interaction>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    //Called by the Event on the trigger collider on the child GO called "InteractionDetector"
    public void OnTriggerChangeDetected(bool entered, GameObject obj)
    {
        if (entered)
            AddPotentialInteraction(obj);
        else
            RemovePotentialInteraction(obj);
    }

    private void AddPotentialInteraction(GameObject obj)
    {
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

        potentialInteractions.Add(newPotentialInteraction);
        RequestUpdateUI(true, newPotentialInteraction.type);
    }

    private void RemovePotentialInteraction(GameObject obj)
    {
        Interaction interaction = potentialInteractions.Find(o => o.interactableObject == obj);
        RequestUpdateUI(false, interaction.type);
    }

    private void RequestUpdateUI(bool visible, InteractionType type)
    {
         UIEvent.OnInterationUI?.Invoke(visible, type);
    }
}
