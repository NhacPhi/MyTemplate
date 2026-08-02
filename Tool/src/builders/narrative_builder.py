import pandas as pd
import config
from src.builders.base_builder import BaseBuilder
from src.models.narrative_models import (
    ActorMode, ChoiceModel, LineModel, DialogueModel,
    DialogueChoiceModel, DialogueNodeModel,
    QuestLinesModel, StepModel, QuestModel, DailyQuestModel
)

class ActorConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing actor config: {self.file_path}")

        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

        if "Actors" in all_sheets:
            df = all_sheets["Actors"]
            actor_data = {}
            for _, row in df.iterrows():
                if pd.isna(row['ID']) : continue

                actor_id = str(row['ID']).strip()

                actor_data[actor_id] = ActorMode(
                    name_hash=self.get_hash(row['Name']),
                    dialogue_default=str(row['DialogueDefault']) if pd.notna(row['DialogueDefault']) else "",
                    location_hash=self.get_hash(row['LocationName']) if 'LocationName' in row and pd.notna(row['LocationName']) else 0
                )
            final_data = {actor_id: actor.to_dict() for actor_id, actor in actor_data.items()}
            self.export_json(config.OUTPUT_GAME_NARRATIVE_FOLDER, final_data, "Actors")

ACTION_TYPE_MAP = {
    "donothing": 0,
    "nextnode": 1,
    "acceptquest": 2,
    "completestep": 3,
    "giveitem": 4,
    "openshop": 5,
    "closedialogue": 6,
    "continuewithstep": 7,
    "winningchoice": 8,
    "losingchoice": 9,
    "incompletestep": 10
}

def parse_action_type(val):
    if pd.isna(val) or val == "":
        return 0
    val_str = str(val).strip()
    if val_str.isdigit():
        return int(val_str)
    return ACTION_TYPE_MAP.get(val_str.lower(), 0)

def get_row_val(row, *possible_keys, default=""):
    for k in possible_keys:
        if k in row and pd.notna(row[k]):
            return str(row[k]).strip()
        k_clean = k.lower().replace(" ", "").replace("_", "")
        for col in row.index:
            col_clean = str(col).lower().replace(" ", "").replace("_", "")
            if (col_clean == k_clean or 
                col_clean == k_clean.replace("dialogue", "dialouge") or 
                col_clean == k_clean.replace("dialouge", "dialogue")):
                if pd.notna(row[col]):
                    return str(row[col]).strip()
    return default

def is_valid_row(row, *possible_keys):
    val = get_row_val(row, *possible_keys)
    return val != ""

class DialogueConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing Dialogue config: {self.file_path}")

        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

        choices_by_node = {} 
        choices_by_line = {}
        if "Choices" in all_sheets:
            df_choices  = all_sheets["Choices"]
            for _, row in df_choices.iterrows():
                if not is_valid_row(row, 'ID', 'ChoiceID', 'NodeID', 'LineID'): continue

                node_or_line_id = get_row_val(row, 'NodeID', 'LineID')
                target_node = get_row_val(row, 'TargetNodeID', 'NextDialogueID')
                action_type_raw = get_row_val(row, 'ActionChoiceType', 'ActionType', 'Type')
                action_type_val = parse_action_type(action_type_raw)
                param_val = get_row_val(row, 'Param')

                node_choice = DialogueChoiceModel(
                    text_hash=self.get_hash(row['Text']) if ('Text' in row and pd.notna(row['Text'])) else 0,
                    action_type=action_type_val,
                    target_node_id=target_node,
                    param=param_val
                )

                if node_or_line_id not in choices_by_node:
                    choices_by_node[node_or_line_id] = []
                choices_by_node[node_or_line_id].append(node_choice)

                legacy_choice = ChoiceModel(
                    text_hash=self.get_hash(row['Text']) if ('Text' in row and pd.notna(row['Text'])) else 0,
                    type=action_type_raw,
                    next_dialogue=target_node,
                )
                if node_or_line_id not in choices_by_line:
                    choices_by_line[node_or_line_id] = []
                choices_by_line[node_or_line_id].append(legacy_choice)

        nodes_by_dialogue = {}

        # 1. Parse from 'Nodes' sheet if present
        if "Nodes" in all_sheets:
            df_nodes = all_sheets["Nodes"]
            for _, row in df_nodes.iterrows():
                if not is_valid_row(row, 'NodeID', 'ID'): continue

                node_id = get_row_val(row, 'NodeID', 'ID')
                dialogue_id = get_row_val(row, 'DialogueID')

                matched_choices = choices_by_node.get(node_id, [])

                node_new = DialogueNodeModel(
                    node_id=node_id,
                    actor_id=get_row_val(row, 'ActorID'),
                    text_hash=self.get_hash(row['Text']) if ('Text' in row and pd.notna(row['Text'])) else 0,
                    next_node_id=get_row_val(row, 'NextNodeID', 'TargetNodeID', 'NextDialogueID'),
                    choices=matched_choices if matched_choices else None
                )

                if dialogue_id not in nodes_by_dialogue:
                    nodes_by_dialogue[dialogue_id] = []
                nodes_by_dialogue[dialogue_id].append(node_new)

        # 2. Fallback: Parse from 'Lines' sheet if 'Nodes' sheet was not provided
        if not nodes_by_dialogue and "Lines" in all_sheets:
            df_lines = all_sheets["Lines"]
            for _, row in df_lines.iterrows():
                if not is_valid_row(row, 'ID', 'LineID', 'NodeID'): continue

                node_id = get_row_val(row, 'NodeID', 'LineID', 'ID')
                dialogue_id = get_row_val(row, 'DialogueID')

                matched_choices = choices_by_node.get(node_id, [])

                node_new = DialogueNodeModel(
                    node_id=node_id,
                    actor_id=get_row_val(row, 'ActorID'),
                    text_hash=self.get_hash(row['Text']) if ('Text' in row and pd.notna(row['Text'])) else 0,
                    next_node_id=get_row_val(row, 'NextNodeID', 'TargetNodeID', 'NextDialogueID'),
                    choices=matched_choices if matched_choices else None
                )

                if dialogue_id not in nodes_by_dialogue:
                    nodes_by_dialogue[dialogue_id] = []
                nodes_by_dialogue[dialogue_id].append(node_new)

        dialogue_data = {}
        if "Dialogues" in all_sheets:
            df_dialogue = all_sheets["Dialogues"]
            for _, row in df_dialogue.iterrows():
                if not is_valid_row(row, 'ID', 'DialogueID'): continue

                dialogue_id = get_row_val(row, 'ID', 'DialogueID')
                match_nodes = nodes_by_dialogue.get(dialogue_id, None)

                dialogue_data[dialogue_id] = DialogueModel(
                    id=dialogue_id,
                    type=get_row_val(row, 'Type'),
                    nodes=match_nodes,
                    lines=None
                )

        final_data = {dialogue_id: dialogue.to_dict() for dialogue_id, dialogue in dialogue_data.items()}
        self.export_json(config.OUTPUT_GAME_NARRATIVE_FOLDER, final_data, "Dialogues")

class QuestLineConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing Questline config: {self.file_path}")

        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

        step_by_quest = {} 
        if "Steps" in all_sheets:
            df_steps  = all_sheets["Steps"]
            for _, row in df_steps.iterrows():
                if pd.isna(row['ID']) : continue

                quest_id = str(row['QuestID']).strip()
                step_id = str(row['ID']).strip()

                new_step = StepModel(
                    id=step_id,
                    actor_id=str(row['ActorID']) if pd.notna(row['ActorID']) else "",
                    previous_dialogue=str(row['DialogueBeforeStep']) if pd.notna(row['DialogueBeforeStep']) else "",
                    completed_dialogue=str(row['CompleteDialogue']) if pd.notna(row['CompleteDialogue']) else "",
                    incomplete_dialogue=str(row['IncompleteDialogue']) if pd.notna(row['IncompleteDialogue']) else "",
                    type=str(row['Type']) if pd.notna(row['Type']) else "",
                    item_id=str(row['ItemID']) if pd.notna(row['ItemID']) else "",
                    target_id=str(row['TargetID']) if ('TargetID' in row and pd.notna(row['TargetID'])) else "",
                    required_amount=int(row['RequiredAmount']) if ('RequiredAmount' in row and pd.notna(row['RequiredAmount'])) else 1,
                )

                if quest_id not in step_by_quest:
                    step_by_quest[quest_id] = []
                step_by_quest[quest_id].append(new_step)

        quest_by_questline = {}
        if "Quests" in all_sheets:
            df_quest = all_sheets["Quests"]
            for _, row in df_quest.iterrows():
                if pd.isna(row['ID']) : continue
                
                quest_id = str(row['ID']).strip()
                questline_id = str(row['QuestLineID']).strip()

                match_steps = step_by_quest.get(quest_id, [])

                quest_type_str = str(row['QuestType']).strip().lower() if 'QuestType' in row and pd.notna(row['QuestType']) else ""
                type_map = {"main": 1, "daily": 2, "none": 0}
                quest_type_val = type_map.get(quest_type_str, 0)

                prereq_str = str(row['PrerequisiteQuestIDs']).strip() if 'PrerequisiteQuestIDs' in row and pd.notna(row['PrerequisiteQuestIDs']) else ""
                prereq_list = [p.strip() for p in prereq_str.split('|') if p.strip()] if prereq_str else []

                new_quest = QuestModel(
                    id=quest_id,
                    chapter_id=str(row['ChapterID']) if 'ChapterID' in row and pd.notna(row['ChapterID']) else "",
                    name_hash=self.get_hash(row['Name']) if ('Name' in row and pd.notna(row['Name'])) else 0,
                    des_hash=self.get_hash(row['Description']) if ('Description' in row and pd.notna(row['Description'])) else 0,
                    prerequisite_quest_ids=prereq_list,
                    required_level=int(row['RequiredLevel']) if 'RequiredLevel' in row and pd.notna(row['RequiredLevel']) else 1,
                    steps=match_steps,
                    quest_type=quest_type_val,
                    reward_id=str(row['RewardID']) if 'RewardID' in row and pd.notna(row['RewardID']) else ""
                )
                if questline_id not in quest_by_questline:
                    quest_by_questline[questline_id] = []
                quest_by_questline[questline_id].append(new_quest)
        
        questline_data = {}
        if "QuestLines" in all_sheets:
            df_questline = all_sheets["QuestLines"]
            for _, row in df_questline.iterrows():
                if not is_valid_row(row, 'ID', 'QuestLineID'): continue
                
                questline_id = get_row_val(row, 'ID', 'QuestLineID')

                match_quest = quest_by_questline.get(questline_id, [])

                questline_data[questline_id] = QuestLinesModel(
                    id=questline_id,
                    name_hash=self.get_hash(row['Name']) if ('Name' in row and pd.notna(row['Name'])) else 0,
                    des_hash=self.get_hash(row['Description']) if ('Description' in row and pd.notna(row['Description'])) else 0,
                    quests=match_quest,
                )

        final_data = {questline_id: questline.to_dict() for questline_id, questline in questline_data.items()}
        self.export_json(config.OUTPUT_GAME_NARRATIVE_FOLDER, final_data, "QuestLines")

class DailyQuestConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing DailyQuest config: {self.file_path}")
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        
        daily_quest_data = {}
        if "DailyQuests" in all_sheets:
            df = all_sheets["DailyQuests"]
            for _, row in df.iterrows():
                if not is_valid_row(row, 'ID', 'QuestID'): continue
                
                quest_id = get_row_val(row, 'ID', 'QuestID')
                
                daily_quest_data[quest_id] = DailyQuestModel(
                    id=quest_id,
                    name_hash=self.get_hash(row['Name']) if ('Name' in row and pd.notna(row['Name'])) else 0,
                    des_hash=self.get_hash(row['Description']) if ('Description' in row and pd.notna(row['Description'])) else 0,
                    target_hash=self.get_hash(row['Target']) if 'Target' in row and pd.notna(row['Target']) else 0,
                    location_hash=self.get_hash(row['LocationName']) if 'LocationName' in row and pd.notna(row['LocationName']) else 0,
                    reward_id=str(row['RewardID']) if 'RewardID' in row and pd.notna(row['RewardID']) else "",
                    objective_type=str(row['ObjectiveType']).strip() if 'ObjectiveType' in row and pd.notna(row['ObjectiveType']) else "",
                    target_id=str(row['TargetID']).strip() if 'TargetID' in row and pd.notna(row['TargetID']) else "",
                    require_amount=int(row['RequireAmount']) if 'RequireAmount' in row and pd.notna(row['RequireAmount']) else 1
                )
                
        if daily_quest_data:
            final_data = {q_id: q.to_dict() for q_id, q in daily_quest_data.items()}
            self.export_json(config.OUTPUT_GAME_NARRATIVE_FOLDER, final_data, "DailyQuests")

