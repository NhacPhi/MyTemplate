import pandas as pd
import config

from src.build import BaseBuilder
from src.models import ActorMode, ChoiceModel, LineModel, DialogueModel, QuestLinesModel, StepModel, QuestModel


class ActorConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing actor config: {self.file_path}")

        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

        # Actor Config
        if "Actors" in all_sheets:
            df = all_sheets["Actors"]
            actor_data = {}
            for _, row, in df.iterrows():
                if pd.isna(row['ID']) : continue

                actor_id = str(row['ID']).strip()

                actor_data[actor_id] = ActorMode(
                    name_hash=self.get_hash(row['Name']),
                    dialogue_default=str(row['DialogueDefault']) if pd.notna(row['DialogueDefault']) else ""                  
                )
        final_data = {actor_id: actor.to_dict() for actor_id, actor in actor_data.items()}
        self.export_json(config.OUTPUT_GAME_NARRATIVE_FOLDER ,final_data, "Actors")

# Build Dialogue Config data        
class DialogueConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):

        print(f"Processing Dialogue config: {self.file_path}")

        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

         # choice
        choices_by_line = {} # Dictionary {"Line_ID": [Choice_01, Choice_02]}}

        if "Choices" in all_sheets:
            df_choices  = all_sheets["Choices"]
            for _, row, in df_choices.iterrows():
                if pd.isna(row['ID']) : continue

                line_id = str(row['LineID']).strip()

                new_choice = ChoiceModel(
                    text_hash=self.get_hash(row['Text']),
                    type=str(row['ActionChoiceType']) if pd.notna(row['ActionChoiceType']) else "",
                    next_dialogue=str(row['NextDialogueID']) if pd.notna(row['NextDialogueID']) else "",
                )

                # group dictionary by Line_ID
                if line_id not in choices_by_line:
                    choices_by_line[line_id] = []
                choices_by_line[line_id].append(new_choice)
        # lines
        lines_by_dialogue = {} # Dictionary {"Dialogue_ID: [Line01, Line02]"}
        if "Lines" in all_sheets:
            df_lines = all_sheets["Lines"]
            for _, row, in df_lines.iterrows():
                if pd.isna(row['ID']) : continue

                line_id = str(row['ID']).strip()
                
                dialogue_id = str(row['DialogueID']).strip()
                
                #Get list choices
                matched_choices = choices_by_line.get(line_id, [])

                line_new = LineModel(
                    text_hash=self.get_hash(row['Text']),
                    actor_id=str(row['ActorID']) if pd.notna(row['ActorID']) else "",
                    choices=matched_choices
                )

                # group dictionary by Line_ID
                if dialogue_id not in lines_by_dialogue:
                    lines_by_dialogue[dialogue_id] = []
                lines_by_dialogue[dialogue_id].append(line_new)

        # dialogue
        dialogue_data = {}
        if "Dialogues" in all_sheets:
            df_dialogue = all_sheets["Dialogues"]
            for _, row, in df_dialogue.iterrows():
                if pd.isna(row['ID']) : continue

                dialogue_id = str(row['ID']).strip()

                match_lines = lines_by_dialogue.get(dialogue_id, [])

                dialogue_data[dialogue_id] = DialogueModel(
                    type=str(row['Type']) if pd.notna(row['Type']) else "",
                    lines=match_lines
                )

        final_data = {dialogue_id: dialogue.to_dict() for dialogue_id, dialogue in dialogue_data.items()}
        self.export_json(config.OUTPUT_GAME_NARRATIVE_FOLDER ,final_data, "Dialogues")

# Build Quesline Config 
class QuestLineConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):

        print(f"Processing Questline config: {self.file_path}")

        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

         # Step Config
        step_by_quest = {} # Dictionary {"quest_line": [Step_01, Step_02]}}

        if "Steps" in all_sheets:
            df_steps  = all_sheets["Steps"]
            for _, row, in df_steps.iterrows():
                if pd.isna(row['ID']) : continue

                quest_id = str(row['QuestID']).strip()

                new_step = StepModel(
                    actor_id=str(row['ActorID']) if pd.notna(row['ActorID']) else "",
                    previous_diagoue=str(row['DialogueBeforeStep']) if pd.notna(row['DialogueBeforeStep']) else "",
                    completed_dialogue=str(row['CompleteDialogue']) if pd.notna(row['CompleteDialogue']) else "",
                    incomplete_dialogue=str(row['IncompleteDialogue']) if pd.notna(row['IncompleteDialogue']) else "",
                    type=str(row['Type']) if pd.notna(row['Type']) else "",
                    item_id=str(row['ItemID']) if pd.notna(row['ItemID']) else "",
                    has_reward=bool(row['HasReward']) if pd.notna(row['HasReward']) else "",
                    reward_id=str(row['RewardID']) if pd.notna(row['RewardID']) else "",
                )

                # group dictionary by quest_id
                if quest_id not in step_by_quest:
                    step_by_quest[quest_id] = []
                step_by_quest[quest_id].append(new_step)

        # Quest Config
        quest_by_questline = {}
        if "Quests" in all_sheets:
            df_quest = all_sheets["Quests"]
            for _, row, in df_quest.iterrows():

                if pd.isna(row['ID']) : continue
                
                quest_id = str(row['ID']).strip()

                questline_id = str(row['QuestLineID']).strip()

                match_steps = step_by_quest.get(quest_id, [])

                new_quest = QuestModel(
                    name_hash=self.get_hash(row['Name']),
                    des_hash=self.get_hash(row['Description']),
                    steps=match_steps,
                )
                # group dictionary by quest_id
                if questline_id not in quest_by_questline:
                    quest_by_questline[questline_id] = []
                quest_by_questline[questline_id].append(new_quest)
        
        # Questline Config
        questline_data = {}
        if "QuestLines" in all_sheets:
            df_questline = all_sheets["QuestLines"]
            for _, row, in df_quest.iterrows():
                if pd.isna(row['ID']) : continue
                
                questline_id = str(row['ID']).strip()

                match_quest = quest_by_questline.get(questline_id, [])

                questline_data[questline_id] = QuestLinesModel(
                    name_hash=self.get_hash(row['Name']),
                    des_hash=self.get_hash(row['Description']),
                    quests=match_quest,
                )

        final_data = {questline_id: questline.to_dict() for questline_id, questline in questline_data.items()}
        self.export_json(config.OUTPUT_GAME_NARRATIVE_FOLDER ,final_data, "QuestLines")

