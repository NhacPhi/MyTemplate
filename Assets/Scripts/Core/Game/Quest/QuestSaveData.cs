using System;
using System.Collections.Generic;

[Serializable]
public class QuestSaveData
{
    public List<string> CompletedQuestLineIDs = new List<string>();
    public List<string> CompletedQuestIDs = new List<string>();
    
    public string ActiveQuestID = string.Empty;
    public int ActiveStepIndex = 0;

    public bool IsQuestCompleted(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return false;
        return CompletedQuestIDs.Contains(questID);
    }

    public bool IsQuestLineCompleted(string questLineID)
    {
        if (string.IsNullOrEmpty(questLineID)) return false;
        return CompletedQuestLineIDs.Contains(questLineID);
    }

    public void CompleteQuest(string questID)
    {
        if (!string.IsNullOrEmpty(questID) && !CompletedQuestIDs.Contains(questID))
        {
            CompletedQuestIDs.Add(questID);
        }
        
        if (ActiveQuestID == questID)
        {
            ActiveQuestID = string.Empty;
            ActiveStepIndex = 0;
        }
    }

    public void CompleteQuestLine(string questLineID)
    {
        if (!string.IsNullOrEmpty(questLineID) && !CompletedQuestLineIDs.Contains(questLineID))
        {
            CompletedQuestLineIDs.Add(questLineID);
        }
    }
}
