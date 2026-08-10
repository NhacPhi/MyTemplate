using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class DialogueManager : MonoBehaviour
{
    //[Inject] GameStateManager gameState;
    [Inject] GameNarrativeData gameNarrativeData;

    private int counterDialogue;
    private int counterLine;
    private int currentNodeIndex;
    private int currentNodeSubLineIndex;
    private bool isNodeTreeMode => currentDialogue != null && currentDialogue.Nodes != null && currentDialogue.Nodes.Count > 0;

    private bool reachedEndOfDialogue => isNodeTreeMode 
        ? (currentDialogue == null || currentNodeIndex >= currentDialogue.Nodes.Count)
        : (currentDialogue == null || currentDialogue.Lines == null || counterDialogue >= currentDialogue.Lines.Count);

    private bool reachedEndOfLine
    {
        get
        {
            if (isNodeTreeMode) return true;
            if (currentDialogue == null || currentDialogue.Lines == null) return true;
            if (counterDialogue >= currentDialogue.Lines.Count) return true;

            var line = currentDialogue.Lines[counterDialogue];
            if (line.Texts == null) return true;
            return counterLine >= line.Texts.Count - 1;
        }
    }

    private DialogueConfig currentDialogue = default;

    private void Awake()
    {
        GameEvent.OnStartDialogue += DisplayDialogueConfig;
        GameEvent.OnEndDialogue += HandleEndDialogue;
    }

    private void OnDestroy()
    {
        GameEvent.OnStartDialogue -= DisplayDialogueConfig;
        GameEvent.OnEndDialogue -= HandleEndDialogue;
        GameEvent.OnAdvanceDialogueEvent -= OnAdvance;
    }

    private void HandleEndDialogue(DialogueType type)
    {
        GameEvent.OnAdvanceDialogueEvent -= OnAdvance;
    }

    public void DisplayDialogueConfig(DialogueConfig config)
    {
        currentDialogue = config;
        counterDialogue = 0;
        counterLine = 0;
        currentNodeIndex = 0;
        currentNodeSubLineIndex = 0;

        if (isNodeTreeMode)
        {
            DisplayCurrentNode();
            return;
        }

        if (currentDialogue == null || currentDialogue.Lines == null || currentDialogue.Lines.Count == 0)
        {
            Debug.LogError("[DialogueManager] Dialogue data is invalid or empty!");
            return;
        }

        ActorConfig actor = gameNarrativeData.GetActorConfig(currentDialogue.Lines[0].ActorID);

        GameEvent.OnAdvanceDialogueEvent -= OnAdvance;
        GameEvent.OnAdvanceDialogueEvent += OnAdvance;

        DisplayDialogueLine(currentDialogue.Lines[0].Texts[0], actor);
    }

    private void DisplayCurrentNode()
    {
        if (currentDialogue == null || currentDialogue.Nodes == null || currentNodeIndex < 0 || currentNodeIndex >= currentDialogue.Nodes.Count)
        {
            DialogueEndAndCloseDialogueUI();
            return;
        }

        GameEvent.OnAdvanceDialogueEvent -= OnAdvance;
        GameEvent.OnAdvanceDialogueEvent += OnAdvance;

        var currentNode = currentDialogue.Nodes[currentNodeIndex];
        if (currentNode.StepEnd || currentNode.IsStepEnd)
        {
            Debug.Log($"[DialogueManager] Node '{currentNode.NodeID}' has StepEnd flag set -> Invoking GameEvent.OnCompleteStep!");
            GameEvent.OnCompleteStep?.Invoke();
        }

        ActorConfig actor = gameNarrativeData.GetActorConfig(currentNode.ActorID);

        string lineText = "";
        if (currentNode.Texts != null && currentNode.Texts.Count > 0)
        {
            if (currentNodeSubLineIndex < 0) currentNodeSubLineIndex = 0;
            if (currentNodeSubLineIndex >= currentNode.Texts.Count) currentNodeSubLineIndex = currentNode.Texts.Count - 1;
            lineText = currentNode.Texts[currentNodeSubLineIndex];
        }
        else
        {
            lineText = !string.IsNullOrEmpty(currentNode.Text) ? currentNode.Text : LocalizationManager.Instance.GetLocalizedValue(currentNode.TextHash);
        }

        DisplayDialogueLine(lineText, actor);
    }

    public void DisplayDialogueLine(string dialogueLine, ActorConfig actor)
    {
        GameEvent.OnOpenDialogue?.Invoke(dialogueLine, actor);
    }

    private void OnAdvance()
    {
        if (isNodeTreeMode)
        {
            var currentNode = currentDialogue.Nodes[currentNodeIndex];

            // 1. If this node has multiple sub-lines split by '|', advance sub-line first
            if (currentNode.Texts != null && currentNodeSubLineIndex < currentNode.Texts.Count - 1)
            {
                currentNodeSubLineIndex++;
                DisplayCurrentNode();
                return;
            }

            // 2. If finished all sub-lines of this node, check choices or next node
            if (currentNode.Choices != null && currentNode.Choices.Count > 0)
            {
                DisplayNodeChoices(currentNode.Choices);
            }
            else if (!string.IsNullOrWhiteSpace(currentNode.NextNodeID))
            {
                JumpToNodeByID(currentNode.NextNodeID);
            }
            else if (currentNodeIndex + 1 < currentDialogue.Nodes.Count)
            {
                currentNodeIndex++;
                currentNodeSubLineIndex = 0;
                DisplayCurrentNode();
            }
            else
            {
                // End of this node branch -> Close Dialogue
                DialogueEndAndCloseDialogueUI();
            }
            return;
        }

        // Legacy Line List execution
        if (reachedEndOfDialogue)
        {
            DialogueEndAndCloseDialogueUI();
            return;
        }

        counterLine++;
        if (!reachedEndOfLine)
        {
            ActorConfig actor = gameNarrativeData.GetActorConfig(currentDialogue.Lines[counterDialogue].ActorID);
            DisplayDialogueLine(currentDialogue.Lines[counterDialogue].Texts[counterLine], actor);
        }
        else if (currentDialogue.Lines[counterDialogue].Choices != null 
            && currentDialogue.Lines[counterDialogue].Choices.Count > 0)
        {
            DisplayChoices(currentDialogue.Lines[counterDialogue].Choices);
        }
        else
        {
            counterDialogue++;
            if (!reachedEndOfDialogue)
            {
                counterLine = 0;
                ActorConfig actor = gameNarrativeData.GetActorConfig(currentDialogue.Lines[counterDialogue].ActorID);
                DisplayDialogueLine(currentDialogue.Lines[counterDialogue].Texts[counterLine], actor);
            }
            else
            {
                counterLine = 0;
                DialogueEndAndCloseDialogueUI();
            }
        }
    }

    private void DisplayChoices(List<ChoiceComponent> choices)
    {
        GameEvent.OnAdvanceDialogueEvent -= OnAdvance;
        GameEvent.OnMakeChoiceUI += MakeDialogueChoice;
        GameEvent.OnShowChoiceUI?.Invoke(choices);
    }

    private void DisplayNodeChoices(List<DialogueChoiceConfig> choices)
    {
        GameEvent.OnAdvanceDialogueEvent -= OnAdvance;

        // Map Node choices to ChoiceComponent for UI compatibility
        List<ChoiceComponent> compatChoices = new List<ChoiceComponent>();
        foreach (var c in choices)
        {
            compatChoices.Add(new ChoiceComponent
            {
                Text = c.TextHash,
                ActionType = c.ActionType,
                TargetNodeID = c.TargetNodeID,
                NextDialogue = c.TargetNodeID,
                Param = c.Param
            });
        }

        GameEvent.OnMakeChoiceUI += MakeDialogueChoice;
        GameEvent.OnShowChoiceUI?.Invoke(compatChoices);
    }

    private void MakeDialogueChoice(ChoiceComponent choice)
    {
        GameEvent.OnMakeChoiceUI -= MakeDialogueChoice;

        string targetNode = (!string.IsNullOrEmpty(choice.TargetNodeID) ? choice.TargetNodeID : choice.NextDialogue)?.Trim();

        switch (choice.ActionType)
        {
            case ChoiceActionType.AcceptQuest:
                GameEvent.OnAcceptQuest?.Invoke(choice.Param);
                break;

            case ChoiceActionType.Reject:
                GameEvent.OnRejectQuest?.Invoke(choice.Param);
                if (string.Equals(targetNode, "step_end", StringComparison.OrdinalIgnoreCase))
                {
                    targetNode = "node_end";
                }
                break;

            case ChoiceActionType.CompleteStep:
            case ChoiceActionType.WinningChoice:
                GameEvent.OnCompleteStep?.Invoke();
                break;

            case ChoiceActionType.ContinueWithStep:
                GameEvent.OnMakeWinChoice?.Invoke();
                break;

            case ChoiceActionType.LosingChoice:
                GameEvent.OnMakeLosingChoice?.Invoke();
                break;

            case ChoiceActionType.CloseDialogue:
            case ChoiceActionType.OpenShop:
            case ChoiceActionType.DoNothing:
            case ChoiceActionType.NextNode:
            case ChoiceActionType.GiveItem:
            case ChoiceActionType.IncompleteStep:
            default:
                break;
        }

        if (!string.IsNullOrEmpty(targetNode))
        {
            JumpToNodeByID(targetNode);
        }
        else
        {
            DialogueEndAndCloseDialogueUI();
        }
    }

    public void JumpToNodeByID(string targetNodeId)
    {
        if (string.IsNullOrEmpty(targetNodeId))
        {
            DialogueEndAndCloseDialogueUI();
            return;
        }

        string cleanTargetId = targetNodeId.Trim();

        if (string.Equals(cleanTargetId, "step_end", StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[DialogueManager] Target node ID is 'step_end' -> Invoking GameEvent.OnCompleteStep!");
            GameEvent.OnCompleteStep?.Invoke();
            DialogueEndAndCloseDialogueUI();
            return;
        }

        if (string.Equals(cleanTargetId, "node_end", StringComparison.OrdinalIgnoreCase))
        {
            DialogueEndAndCloseDialogueUI();
            return;
        }

        if (isNodeTreeMode)
        {
            int index = currentDialogue.Nodes.FindIndex(n => n.NodeID != null && n.NodeID.Trim().Equals(cleanTargetId, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                currentNodeIndex = index;
                currentNodeSubLineIndex = 0;
                DisplayCurrentNode();
                return;
            }
        }

        // If not found in current tree, check external DialogueConfig
        DialogueConfig nextDialogue = gameNarrativeData.GetDialogueConfigByID(cleanTargetId);
        if (nextDialogue != null)
        {
            DisplayDialogueConfig(nextDialogue);
        }
        else
        {
            Debug.LogWarning($"[DialogueManager] Dialogue ended because target Node/Dialogue ID '{cleanTargetId}' (requested by Node '{currentDialogue?.Nodes?[currentNodeIndex]?.NodeID}') WAS NOT FOUND in current tree or DialogueConfigs!");
            DialogueEndAndCloseDialogueUI();
        }
    }

    private void DialogueEndAndCloseDialogueUI()
    {
        GameEvent.OnEndDialogue?.Invoke(currentDialogue != null ? currentDialogue.Type : DialogueType.Default);
        GameEvent.OnAdvanceDialogueEvent -= OnAdvance;
    }
}
