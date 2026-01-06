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


    private const string ActorAddress = "Actors";
    private const string QuestLineAddress = "QuestLines";
    private const string DialogueAddress = "Dialogues";

    public async UniTask LoadGameNarrativeConfig(CancellationToken cancellationToken = default)
    {
        // 1. Load JSON
        var (actorText, dialogueText, questLineText) = await UniTask.WhenAll(
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(ActorAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(DialogueAddress, token: cancellationToken),
            AddressablesManager.Instance.LoadAssetAsync<TextAsset>(QuestLineAddress, token: cancellationToken)
        );

        ActorConfigs = Json.DeserializeObject<Dictionary<string, ActorConfig>>(actorText.text);
        DialogueConfigs = Json.DeserializeObject<Dictionary<string, DialogueConfig>>(dialogueText.text);
        QuestLineConfigs = Json.DeserializeObject<Dictionary<string, QuestLineConfig>>(questLineText.text);

        AddressablesManager.Instance.RemoveAsset(ActorAddress);
        AddressablesManager.Instance.RemoveAsset(QuestLineAddress);
        AddressablesManager.Instance.RemoveAsset(DialogueAddress);

        foreach (var dialogue in DialogueConfigs.Values)
        {
            foreach(var line in dialogue.Lines)
            {
                string localizedRaw = LocalizationManager.Instance.GetLocalizedValue(line.Text);

                if(!string.IsNullOrEmpty(localizedRaw))
                {
                    line.Texts = localizedRaw.Split('|').ToList();
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
        return DialogueConfigs.GetValueOrDefault(dialogueID);
    }
}
