using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading;

public class GameNarrativeData : MonoBehaviour 
{
    [SerializeField] private List<ActorSO> actorSOs;

    private List<ActorData> actorDatas = new();
    private List<ChoiceData> choiceDatas = new();
    private List<LineData> lineDatas = new();
    private List<DialogueData> dialogueDatas = new();

    private List<RewardPayLoad> rewards = new();
    private List<StepData> steps = new();
    private List<QuestData> quests = new();
    private List<QuestLineData> questLines = new();

    public List<QuestLineData> QuestLines => questLines;

    // Dict Cache
    private Dictionary<string, ActorSO> actorSoDict;

    public async UniTask LoadGameNarrativeConfig(CancellationToken cancellationToken = default)
    {
        var tasks = new List<UniTask>()
        {
            LoadActorData("Actors", cancellationToken),
            LoadChoiceData("Choices", cancellationToken),
            LoadLineData("Lines", cancellationToken),
            LoadDialogueData("Dialogues", cancellationToken),

            LoadRewardData("Rewards", cancellationToken),
            LoadStepData("Steps", cancellationToken),
            LoadQuestData("Quests", cancellationToken),
            LoadQuestLineData("QuestLines", cancellationToken)
        };
        await UniTask.WhenAll(tasks);
    }
    
    private async UniTask LoadActorData(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        var actorConfigs = Json.DeserializeObject<List<ActorConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);

        var actorSoDict = actorSOs.ToDictionary(a => a.ID, a => a);

        if (actorConfigs.Count > 0)
        {
            foreach (var actor in actorConfigs)
            {
                if (!actorSoDict.TryGetValue(actor.ID, out var so))
                {
                    Debug.LogWarning($"ActorSO missing for {actor.ID}");
                    continue;
                }
                var actorData = new ActorData();
                actorData.InitData(actor.ID, actor.Name, so.Texture);
                actorDatas.Add(actorData);
            }
        }
    }
    private async UniTask LoadChoiceData(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        var choiceConfigs = Json.DeserializeObject<List<ChoiceConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);

        if (choiceConfigs.Count > 0)
        {
            foreach (var choice in choiceConfigs)
            {
                var choiceData = new ChoiceData();
                choiceData.InitData(choice.ID, choice.LineID, choice.Text, choice.ActionType, choice.NextDialogueID);
                choiceDatas.Add(choiceData);
            }
        }

    }

    private async UniTask LoadLineData(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        var lineConfigs = Json.DeserializeObject<List<LineConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);

        var choiceLookup = choiceDatas.GroupBy(c => c.LineID)
                          .ToDictionary(g => g.Key, g => g.ToList());
        if (lineConfigs.Count > 0)
        {
            foreach (var line in lineConfigs)
            {
                var lineData = new LineData();
                string text = LocalizationManager.Instance.GetLocalizedValue(line.Texts);
                List<string> texts = text.Split('|').ToList();

                choiceLookup.TryGetValue(line.ID, out var choices);

                lineData.InitData(line.ID, line.DialogueID, line.ActorID, texts, choices ?? new List<ChoiceData>());

                lineDatas.Add(lineData);
            }
        }
    }
    private async UniTask LoadDialogueData(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        var dialogueConfigs = Json.DeserializeObject<List<DialogueConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
 
        var lineLookup = lineDatas.GroupBy(l => l.DialogueID)
                      .ToDictionary(g => g.Key, g => g.ToList());
        if (dialogueConfigs.Count > 0)
        {
            foreach (var dialogue in dialogueConfigs)
            {
                var dialogueData = new DialogueData();
                lineLookup.TryGetValue(dialogue.ID, out var lines);
                dialogueData.InitData(dialogue.ID, dialogue.Type, lines ?? new List<LineData>(), dialogue.ActorID);
                dialogueDatas.Add(dialogueData);
            }
        }

    }
    private async UniTask LoadRewardData(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        rewards = Json.DeserializeObject<List<RewardPayLoad>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);
    }
    private async UniTask LoadStepData(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        var stepConfigs = Json.DeserializeObject<List<StepConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);

        var dialogueDict = dialogueDatas.ToDictionary(a => a.ID, a => a);
        var rewardDict = rewards.ToDictionary(r => r.QuestID, r => r);
        if (stepConfigs.Count > 0)
        {
            foreach(var step in stepConfigs)
            {
                var stepData = new StepData();
                dialogueDict.TryGetValue(step.DialogueBeforeStep, out var dialogueBeforStep);
                dialogueDict.TryGetValue(step.CompleteDialogue, out var completeDialogue);
                dialogueDict.TryGetValue(step.IncompleteDialogue, out var inCompleteDialogue);
                rewardDict.TryGetValue(step.RewardID, out var reward);
                stepData.InitData(step.ID, step.QuestID, step.ActorID, dialogueBeforStep, completeDialogue, inCompleteDialogue,
                    step.Type, step.RewardID, step.HasReward, reward, step.ID);

                steps.Add(stepData);
            }
        }
    }
    private async UniTask LoadQuestData(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        var questConfigs = Json.DeserializeObject<List<QuestConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);

        var stepLookup = steps.GroupBy(s => s.QuestID)
                      .ToDictionary(g => g.Key, g => g.ToList());
        if (questConfigs.Count > 0)
        {
            foreach(var quest in questConfigs)
            {
                QuestData questData = new QuestData();
                stepLookup.TryGetValue(quest.ID, out var steps);
                questData.InitData(quest.ID, quest.QuestLineID, steps, quest.EventID);
                quests.Add(questData);
            }
        }
    }

    private async UniTask LoadQuestLineData(string key, CancellationToken cancellationToken = default)
    {
        var textAsset = await AddressablesManager.Instance.LoadAssetAsync<TextAsset>(key, token: cancellationToken);
        var questLineConfigs = Json.DeserializeObject<List<QuestLineConfig>>(textAsset.text);
        AddressablesManager.Instance.RemoveAsset(key);

        var questLookup = quests.GroupBy(q => q.QuestLineID)
                      .ToDictionary(g => g.Key, g => g.ToList());
        if (questLineConfigs.Count > 0)
        {
            foreach (var questLine in questLineConfigs)
            {
                QuestLineData questLineData = new QuestLineData();
                questLookup.TryGetValue(questLine.ID, out var quests);
                questLineData.InitData(questLine.ID, quests, questLine.EventID);
                questLines.Add(questLineData);
            }
        }
    }
    public ActorData GetActorData(string id)
    {
        return actorDatas.Find(o => o.ID == id);
    }

    public DialogueData GetDefaultDialogueDataByActorID(string id)
    {
        return dialogueDatas.Find(d => (d.ActorID == id && d.Type == DialogueType.Default));
    }

    public DialogueData GetDialogueDataByID(string id)
    {
        return dialogueDatas.Find(d => d.ID == id);
    }
}
