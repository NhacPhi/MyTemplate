using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueChoicesUIManager : MonoBehaviour
{
    [SerializeField] private DialogueChoiceUI[] choiceButtons;
    
    public void FillChoices(List<ChoiceData> choices)
    {
        if (choices != null)
        {
            int maxCount = Mathf.Max(choices.Count, choiceButtons.Length);

            for (int i = 0; i < maxCount; i++)
            {
                if (i < choiceButtons.Length)
                {
                    if (i < choices.Count)
                    {
                        choiceButtons[i].gameObject.SetActive(true);
                        choiceButtons[i].FillChoice(choices[i]);
                    }
                    else
                    {
                        choiceButtons[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogError("There are more choices than buttons");
                }
            }

        }
    }

    public void DisableAllCHoiceUI()
    {
        foreach(var choice in choiceButtons) { 
            choice.RemoveEventOfButton();
        }
    }
}
