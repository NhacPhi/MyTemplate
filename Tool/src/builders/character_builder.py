import pandas as pd
import config
from src.builders.base_builder import BaseBuilder
from src.models.character_models import CharacterModel, SkillComponent, AttributeComponent

class CharacterConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path
    def run(self):
        print(f"Processing character config: {self.file_path}")
        
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

        skill_lookup = {}
        if "SkillConfig" in all_sheets:
            df_skills = all_sheets["SkillConfig"]
            for _, row in df_skills.iterrows():
                if pd.isna(row['ID']): continue
            
                skill_id = str(row['ID']).strip()
                
                skill_lookup[skill_id] = SkillComponent(
                    id=skill_id,
                    name_hash=self.get_hash((row['Name'].strip())) if not pd.isna(row['Name']) else 0,
                    des_hash=self.get_hash((row['Des']).strip()) if not pd.isna(row['Des']) else 0,
                    skill=str(row['Type']).strip() if not pd.isna(row['Type']) else "None",
                    skill_type=str(row['SkillType']).strip() if not pd.isna(row['SkillType']) else "None",
                    target_type=str(row['TargetType']).strip() if not pd.isna(row['TargetType']) else "None",
                    damage_multiplier=float(row['DamageMultiplier']) if not pd.isna(row['DamageMultiplier']) else 0.0,
                    max_cooldown=int(row['MaxCooldown']) if not pd.isna(row['MaxCooldown']) else 0,
                    flat_damage=float(row['FlatDamage']) if not pd.isna(row['FlatDamage']) else 0,
                    effect_id=str(row['Effect']).strip() if not pd.isna(row['Effect']) else "None",
                    sound=str(row['Sound']).strip() if not pd.isna(row['Sound']) else ""
                )

        character_data = {}
        if "Character" in all_sheets:
            df = all_sheets["Character"]
            for _, row in df.iterrows():
                if pd.isna(row['ID']) : continue

                character_id = str(row['ID']).strip()

                base_id = str(row['Base']).strip() if not pd.isna(row['Base']) else None
                major_id = str(row['Major']).strip() if not pd.isna(row['Major']) else None
                ultimate_id = str(row['Ultimate']).strip() if not pd.isna(row['Ultimate']) else None

                skill_data = {}
                if base_id and base_id in skill_lookup:
                    skill_data["Base"] = skill_lookup[base_id]
                if major_id and major_id in skill_lookup:
                    skill_data["Major"] = skill_lookup[major_id]
                if ultimate_id and ultimate_id in skill_lookup:
                    skill_data["Ultimate"] = skill_lookup[ultimate_id]

                character_data[character_id] = CharacterModel(
                    name_hash=self.get_hash(row['Name']),
                    rare=str(row['Rare']),
                    type=str(row['Type']),
                    skills=skill_data
                )

        if "CharacterStat" in all_sheets:
            df = all_sheets["CharacterStat"]
            stat_columns = ['hp', 'def', 'atk', 'speed']
            
            for _, row in df.iterrows():
                char_id = str(row['ID']).strip()
                if char_id in character_data:
                    char_stats = {}
                    for col in stat_columns:
                        if col in df.columns:
                            char_stats[col] = int(row[col]) if pd.notna(row[col]) else 0
                    character_data[char_id].stats = char_stats

        if "CharacterAttribute" in all_sheets:
            df = all_sheets["CharacterAttribute"]

            for _, row in df.iterrows():
                char_id = str(row['ID']).strip()
                if char_id in character_data:
                    attr_type = str(row['AttributeType']).strip()

                    attr_comp = AttributeComponent(
                        max_stat_type = str(row['StatLink']).strip() if pd.notna(row['StatLink']) else "None",
                        start_percent = float(row['StartPercent']) if pd.notna(row['StartPercent']) else 0.0
                    )
                    character_data[char_id].attributes[attr_type] = attr_comp

        if "CharacterUpgrade" in all_sheets:
            df = all_sheets["CharacterUpgrade"]
            stat_columns = ['hp', 'def', 'atk']
            
            for _, row in df.iterrows():
                char_id = str(row['ID']).strip()
                if char_id in character_data:
                    char_stats = {}
                    for col in stat_columns:
                        if col in df.columns:
                            char_stats[col] = int(row[col]) if pd.notna(row[col]) else 0
                    character_data[char_id].upgrades = char_stats
        
        final_data = {character_id: character.to_dict() for character_id, character in character_data.items()}
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_data, "CharacterConfig")
