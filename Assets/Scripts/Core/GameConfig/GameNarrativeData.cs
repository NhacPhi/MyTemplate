using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Linq;

public class GameNarrativeData : MonoBehaviour 
{
    [SerializeField] private List<ActorSO> actorSOs;

    public Dictionary<string, ActorConfig> ActorConfigs = new();
    public Dictionary<string, DialogueConfig> DialogueConfigs = new();
    public Dictionary<string, QuestLineConfig> QuestLineConfigs = new();
    public Dictionary<string, DailyQuestConfig> DailyQuestConfigs = new();


    private const string ActorAddress = "Actors";
    private const string QuestLineAddress = "QuestLines";
    private const string DialogueAddress = "Dialogues";
    private const string DailyQuestAddress = "DailyQuests";

    public async UniTask LoadGameNarrativeConfig(CancellationToken cancellationToken = default)
    {
        // 1. Load JSON
        var (actorText, dialogueText, questLineText, dailyQuestText) = await UniTask.WhenAll(
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(ActorAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(DialogueAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(QuestLineAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(DailyQuestAddress, token: cancellationToken)
        );

        ActorConfigs = Json.DeserializeObject<Dictionary<string, ActorConfig>>(actorText.text);
        DialogueConfigs = Json.DeserializeObject<Dictionary<string, DialogueConfig>>(dialogueText.text);
        QuestLineConfigs = Json.DeserializeObject<Dictionary<string, QuestLineConfig>>(questLineText.text);
        if (dailyQuestText != null)
        {
            DailyQuestConfigs = Json.DeserializeObject<Dictionary<string, DailyQuestConfig>>(dailyQuestText.text);
            AddressablesManager.Instance.RemoveAsset(DailyQuestAddress);
        }

        AddressablesManager.Instance.RemoveAsset(ActorAddress);
        AddressablesManager.Instance.RemoveAsset(QuestLineAddress);
        AddressablesManager.Instance.RemoveAsset(DialogueAddress);

        foreach (var dialogue in DialogueConfigs.Values)
        {
            if (dialogue.Nodes != null && dialogue.Nodes.Count > 0)
            {
                foreach (var node in dialogue.Nodes)
                {
                    node.Text = LocalizationManager.Instance.GetLocalizedValue(node.TextHash);
                    if (!string.IsNullOrEmpty(node.Text))
                    {
                        node.Texts = node.Text.Split('|').ToList();
                    }
                }
            }

            if (dialogue.Lines != null)
            {
                foreach (var line in dialogue.Lines)
                {
                    string localizedRaw = LocalizationManager.Instance.GetLocalizedValue(line.Text);

                    if (!string.IsNullOrEmpty(localizedRaw))
                    {
                        line.Texts = localizedRaw.Split('|').ToList();
                    }
                }
            }
        }

        foreach(var actor in ActorConfigs)
        {
            actor.Value.ActorSo = actorSOs.Find(o => o.ID == actor.Key);
        }
    }

    public ActorConfig GetActorConfig(string actorID)
    {
        return ActorConfigs.GetValueOrDefault(actorID);
    }

    public ActorSO GetActorSO(string actorID)
    {
        return actorSOs.Find(o => o.ID == actorID);
    }

    public DialogueConfig GetDefaultDialogueConfigByActorID(string actorID)
    {
        ActorConfig actorConfig = ActorConfigs.GetValueOrDefault(actorID);
        return DialogueConfigs.GetValueOrDefault(actorConfig.DialogueDefault);
    }

    public DialogueConfig GetDialogueConfigByID(string dialogueID)
    {
        if (string.IsNullOrEmpty(dialogueID)) return null;

        string cleanID = dialogueID.Trim();
        if (DialogueConfigs.TryGetValue(cleanID, out var config))
        {
            return config;
        }

        var match = DialogueConfigs.FirstOrDefault(x => string.Equals(x.Key.Trim(), cleanID, StringComparison.OrdinalIgnoreCase));
        if (match.Value != null)
        {
            return match.Value;
        }

        // Fallback: Check if cleanID is a NodeID inside any DialogueConfig's Nodes list
        foreach (var dlg in DialogueConfigs.Values)
        {
            if (dlg != null && dlg.Nodes != null)
            {
                if (dlg.Nodes.Any(n => n.NodeID != null && string.Equals(n.NodeID.Trim(), cleanID, StringComparison.OrdinalIgnoreCase)))
                {
                    return dlg;
                }
            }
        }

        return null;
    }
}
