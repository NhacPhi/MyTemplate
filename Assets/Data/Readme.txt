DialogueConfig(ID, DialogueType)
LineConfig(ID, DialogID, ActorID, Texts)
ChoiceConfig(ID, LineID, ActionType, Text,NextDialougeID)

QuestLine
QuestConfig(ID, Name, QuestLineID)
StepConfig(ID, QuestID, StepType, Item, HasReward, ItemReward)

ChoiceData(ID, Text, ActionType, NextDialogueID)
LineData(ID, ActorID, List<string> texts, List<ChoiceData> choices)
DialogueData(ID, ActionType, List<LineData> lines, Action EndOfDialog)

StepData(ID, ActorID, Type(Dialogue), DialogueBeforID, Complete Dialogue, InComplete Dialogue, ItemID, bool HasReward, count, bool isDone, Action EndOfStep)
QuestData(ID, Name, List<Step>, Action EndOfQuest)
QuestLine(ID, Name, List<QuestData>, Action EnofQuestLine)

Dialogue (Cuộc hội thoại giữa 2 ng) - Một Dialogue Có nhiều Line
Line (Mội mội lời thoại của một nhận vật tương ứng với 1 line (Có thể nhân vật nói ít câu or nhiều câu) List<string> texts;
Choice (Hiện thị lựa chọn, cuộc hội thoại sẽ rẽ sang một Dialogue mới với mỗi một sửa lựa chọn).

Player: Detect Talk
NPC: Trời ơi yêu quái. Xin tha mang.
Player: Nguơi bảo ai là yêu quá.
Sư phụ: Ngộ Không không được vô lễ. Ngươi mau đi hỏi đường đi.
Player: Option : KHông biết đây là núi gì, trang gì?
		 Không biết gần đây có con yêu quái nào không?
		 Ngươi đang đi đâu?
		 Đã làm phiền rồi.




 
