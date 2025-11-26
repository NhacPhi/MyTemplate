using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tech.Json;
using System.IO;
using System.Linq;

public class GameNarrativeData : MonoBehaviour 
{
    [SerializeField] private List<ActorSO> actorSOs;

    private List<ActorConfig> actorConfigs = new();
    private List<DialogueConfig> dialogueConfigs = new();
    private List<LineConfig> lineConfigs = new();
    private List<ChoiceConfig> choiceConfigs = new();

    private List<ActorData> actorDatas = new();
    private List<ChoiceData> choiceDatas = new();
    private List<LineData> lineDatas = new();
    private List<DialogueData> dialogueDatas = new();

    public void Init()
    {
        LoadGameNarrativeConfig();
        Debug.Log("DialogueData: " + dialogueDatas);
    }

    private void LoadGameNarrativeConfig()
    {
        string path = "Assets/Data/Narrative/";

        string fileActor = "Actor.json";
        string fileDialogue = "Dialogues.json";
        string fileLine = "Lines.json";
        string fileChoice = "Choices.json";

        Json.LoadJson(Path.Combine(path, fileActor), out actorConfigs);
        Json.LoadJson(Path.Combine(path, fileDialogue), out dialogueConfigs);
        Json.LoadJson(Path.Combine(path, fileLine), out lineConfigs);
        Json.LoadJson(Path.Combine(path, fileChoice), out choiceConfigs);

        if(actorConfigs.Count > 0)
        {
            foreach(var actor in actorConfigs)
            {
                var actorData = new ActorData();
                Sprite sprite = actorSOs.Find(a => a.ID == actor.ID).Texture;
                actorData.InitData(actor.ID, actor.Name, sprite);
                actorDatas.Add(actorData);
            }
        }


        if(choiceConfigs.Count > 0)
        {
            foreach(var choice in choiceConfigs)
            {
                var choiceData = new ChoiceData();
                choiceData.InitData(choice.ID,choice.LineID, choice.Text, choice.ActionType, choice.NextDialogueID);
                choiceDatas.Add(choiceData);
            }
        }

        if(lineConfigs.Count > 0)
        {
            foreach(var line in lineConfigs)
            {
                var lineData = new LineData();
                string text = LocalizationManager.Instance.GetLocalizedValue(line.Texts);
                List<string> texts = text.Split('|').ToList();
                List<ChoiceData> datas = new();
                if(line.HasChoice)
                {
                    datas = choiceDatas.Where(c => c.LineID == line.ID).ToList();
                }
                lineData.InitData(line.ID,line.DialogueID, line.ActorID, texts, datas);
                lineDatas.Add(lineData);
            }
        }


        if (dialogueConfigs.Count > 0)
        {
            foreach (var dialogue in dialogueConfigs)
            {
                var dialogueData = new DialogueData();
                List<LineData> lines = new List<LineData>();
                foreach (var line in lineDatas)
                {
                    if (line.DialogueID == dialogue.ID)
                    {
                        lines.Add(line);
                    }
                }
                dialogueData.InitData(dialogue.ID, dialogue.Type, lines, dialogue.ActorID);
                dialogueDatas.Add(dialogueData);
            }
        }
    }

    public ActorData GetActorData(string id)
    {
        return actorDatas.Find(o => o.ID == id);
    }

    public DialogueData GetDialogueDataByActorID(string id)
    {
        return dialogueDatas.Find(d => d.ActorID == id);
    }

    public DialogueData GetDialogueDataByID(string id)
    {
        return dialogueDatas.Find(d => d.ID == id);
    }
}
