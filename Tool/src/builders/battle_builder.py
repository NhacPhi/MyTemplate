import pandas as pd
import config
from src.builders.base_builder import BaseBuilder
from src.models.battle_models import EffectSkillModel, BattleModel, StageEnemiesCompoment

class EffectConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path
    
    def run(self):
        print(f"Processing effect config: {self.file_path}")
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        
        effect_skill_data = {}
        if "EffectConfig" in all_sheets:
            df_battle = all_sheets["EffectConfig"]

            for _, row in df_battle.iterrows():
                effect_id = str(row['ID']).strip()
                if pd.isna(effect_id) or not effect_id:
                    continue             

                effect_skill_data[effect_id] = EffectSkillModel(
                    name_hash=self.get_hash((row['Name'].strip())) if pd.notna(row['Name']) else 0,
                    des_hash=self.get_hash((row['Des'].strip())) if pd.notna(row['Des']) else 0,
                    type=str(row['Type']).strip() if pd.notna(row['Type']) else "None",
                    target_stat=str(row['TargetStat']).strip() if pd.notna(row['TargetStat']) else "None",
                    modify_type = str(row['ModifyType']).strip() if pd.notna(row['ModifyType']) else "None",
                    duration=int(row['Duration']) if pd.notna(row['Duration']) else 0,
                    value=int(row['Value']) if pd.notna(row['Value']) else 0,
                    max_stack=int(row['Value']) if pd.notna(row['Value']) else 0
                )
            
        final_effect_data = {e_id: effect.to_dict() for e_id, effect in effect_skill_data.items()}
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_effect_data, "EffectConfig")

class BattleConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path
    
    def run(self):
        print(f"Processing battle config: {self.file_path}")
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        
        battle_data = {}
        if "BattleConfig" in all_sheets:
            df_battle = all_sheets["BattleConfig"]

            for _, row in df_battle.iterrows():
                battle_id = str(row['ID']).strip()
                if pd.isna(battle_id) or not battle_id:
                    continue

                battle_data[battle_id] = BattleModel(
                    name_hash=self.get_hash(row['Name']),
                    background = str(row['BackGround']).strip() if pd.notna(row['BackGround']) else "",
                    reward = str(row['Reward']).strip() if pd.notna(row['Reward']) else "",
                    enemies=[] 
                )

        if "StageEnemies" in all_sheets:
            df_enemies = all_sheets["StageEnemies"]

            for _, row in df_enemies.iterrows():
                battle_id = str(row['BattleID']).strip()

                if battle_id in battle_data:
                    enemy_comp = StageEnemiesCompoment(
                        slot = int(row['Slot']) if pd.notna(row['Slot']) else 1,
                        enemy_id = str(row['Enemy_ID']).strip() if pd.notna(row['Enemy_ID']) else "",
                        enemy_level = int(row['Level']) if pd.notna(row['Level']) else 1,
                        boss = bool(row['Boss']) if pd.notna(row['Boss']) else False
                    )
                    battle_data[battle_id].enemies.append(enemy_comp)

        final_battle_data = {b_id: battle.to_dict() for b_id, battle in battle_data.items()}
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_battle_data, "BattleConfig")
