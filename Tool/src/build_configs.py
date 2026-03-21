import pandas as pd

import config
from src.build import BaseBuilder
from src.models import ItemModel, WeaponComponent, ExpComponent, ArmorComponent, SkillComponent, CharacterModel, EffectSkillModel, AttributeComponent, BattleModel, StageEnemiesCompoment

class ItemConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing item config: {self.file_path}")
        
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

        # Item_Master
        if "Item_Master" in all_sheets:
            df = all_sheets["Item_Master"]
            master_data = {}
            for _, row, in df.iterrows():
                if pd.isna(row['ID']) : continue

                item_id = str(row['ID']).strip()

                master_data[item_id] = ItemModel(
                    name_hash=self.get_hash(row['Name']),
                    description_hash=self.get_hash(row['Description']),
                    use_hash=self.get_hash(row['UseDescription']),
                    item_type=str(row['ItemType']),
                    rarity=str(row['Rarity'])
                )


        # --- 3. Handle Weapon item data ---
        if "Weapon" in all_sheets:
            df_weapon = all_sheets["Weapon"]
            for _, row in df_weapon.iterrows():
                w_id = str(row['ID']).strip()
                if w_id in master_data and master_data[w_id].item_type == "Weapon":

                    master_data[w_id].weapon_data = WeaponComponent(
                        weapon_type= str(row['Type']) if pd.notna(row['Type']) else "None",
                        stats={
                            "hp": int(row['HP']) if pd.notna(row['HP']) else 0,
                            "atk": int(row['ATK']) if pd.notna(row['ATK']) else 0,
                        },
                        upgrades={
                            "hp": int(row['GrowthHP']) if pd.notna(row['GrowthHP']) else 0,
                            "atk": int(row['GrowthATK']) if pd.notna(row['GrowthATK']) else 0,
                        }
                    )

        # --- 3. Handle armor item data ---
        if "Armor" in all_sheets:
            df_weapon = all_sheets["Armor"]
            for _, row in df_weapon.iterrows():
                w_id = str(row['ID']).strip()
                if w_id in master_data and master_data[w_id].item_type == "Armor":

                    master_data[w_id].armor_data = ArmorComponent(
                        part=str(row['Part']) if pd.notna(row['Part']) else "",
                        armor_set=str(row['SetArmor']) if pd.notna(row['SetArmor']) else "",
                    )                  

        # --- 4. Handle item detail data ---
        if "Item_Detail" in all_sheets:
            df_item_detail = all_sheets["Item_Detail"]
            for _, row in df_item_detail.iterrows():
                w_id = str(row['ID']).strip()
                if w_id in master_data and master_data[w_id].item_type == "Exp":
                    master_data[w_id].exp_data = ExpComponent(
                        value=int(row['Pram01']) if pd.notna(row['Pram01']) else 0
                    )


        final_data = {item_id: item.to_dict() for item_id, item in master_data.items()}
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER ,final_data, "ItemConfig")

class CharacterConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path
    def run(self):
        print(f"Processing character config: {self.file_path}")
        
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

    # Skill Config
        skill_lookup = {}
        if "SkillConfig" in all_sheets:
            df_skills = all_sheets["SkillConfig"]
            for _, row in df_skills.iterrows():
                if pd.isna(row['ID']): continue
            
                skill_id = str(row['ID']).strip()
                
                # Đóng gói dữ liệu thành SkillComponent
                skill_lookup[skill_id] = SkillComponent(
                    id=skill_id,
                    name_hash=self.get_hash((row['Name'].strip())) if not pd.isna(row['Name']) else "",
                    des_hash=self.get_hash((row['Des']).strip()) if not pd.isna(row['Des']) else "",
                    skill=str(row['Type']).strip() if not pd.isna(row['Type']) else "None",
                    target_type=str(row['TargetType']).strip() if not pd.isna(row['TargetType']) else "None",
                    damage_multiplier=float(row['DamageMultiplier']) if not pd.isna(row['DamageMultiplier']) else 0.0,
                    max_cooldown=int(row['MaxCooldown']) if not pd.isna(row['MaxCooldown']) else 0,
                    flat_damage=float(row['FlatDamage']) if not pd.isna(row['FlatDamage']) else 0,
                    effect_id=str(row['Effect']).strip() if not pd.isna(row['Effect']) else "None"
                )

        character_data = {}
        # CharacterConfig
        if "Character" in all_sheets:
            df = all_sheets["Character"]
            for _, row, in df.iterrows():
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

        # Handle character stats
        if "CharacterStat" in all_sheets:
            df = all_sheets["CharacterStat"]

            # defind clomums
            stat_columns = ['hp', 'def', 'atk', "speed"]
            
            for _, row, in df.iterrows():
                char_id = str(row['ID']).strip()
                if char_id in character_data:
                    char_stats = {}
                    for col in stat_columns:
                        if col in df.columns:
                            char_stats[col] = int(row[col]) if not pd.isna(row[col]) else 0
                    character_data[char_id].stats = char_stats

        # Handle character attribute
        if "CharacterAttribute" in all_sheets:
            df = all_sheets["CharacterAttribute"]

            for _, row, in df.iterrows():
                char_id = str(row['ID']).strip()
                if char_id in character_data:
                    attr_type = str(row['AttributeType']).strip()

                    attr_comp = AttributeComponent(
                        max_stat_type = str(row['StatLink']).strip() if not pd.isna(row['StatLink']) else "None",
                        start_percent = float(row['StartPercent']) if not pd.isna(row['StartPercent']) else 0.0
                    )
                character_data[char_id].attributes[attr_type] = attr_comp

        # Handle character upgrade
        if "CharacterUpgrade" in all_sheets:
            df = all_sheets["CharacterUpgrade"]

            # defind clomums
            stat_columns = ['hp', 'def', 'atk']
            
            for _, row, in df.iterrows():
                char_id = str(row['ID']).strip()
                if char_id in character_data:
                    char_stats = {}
                    for col in stat_columns:
                        if col in df.columns:
                            char_stats[col] = int(row[col]) if not pd.isna(row[col]) else 0
                    character_data[char_id].upgrades = char_stats
        
        final_data = {character_id: character.to_dict() for character_id, character in character_data.items()}
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_data, "CharacterConfig")

        # Effect Skill Config
        effect_skill_data = {}
        if "EffectConfig" in all_sheets:
            df_battle = all_sheets["EffectConfig"]

            for _, row in df_battle.iterrows():
                effect_id = str(row['ID']).strip()
                if pd.isna(effect_id) or not effect_id:
                    continue             

                name_hash=self.get_hash((row['Name'].strip())) if not pd.isna(row['Name']) else ""
                des_hash=self.get_hash((row['Des'].strip())) if not pd.isna(row['Des']) else ""
                type=str(row['Type']).strip() if pd.notna(row['Type']) else "None"
                target_stat=str(row['TargetStat']).strip() if pd.notna(row['TargetStat']) else "None"
                modify_type = str(row['ModifyType']).strip() if pd.notna(row['ModifyType']) else "None"
                duration=int(row['Duration']) if not pd.isna(row['Duration']) else 0
                value=int(row['Value']) if not pd.isna(row['Value']) else 0
                max_stack=int(row['Value']) if not pd.isna(row['Value']) else 0

                effect_skill_data[effect_id] = EffectSkillModel(
                    name_hash=name_hash,
                    des_hash=des_hash,
                    type=type,
                    target_stat=target_stat,
                    modify_type=modify_type,
                    duration=duration,
                    value=value,
                    max_stack=max_stack
                )
            
        final_effect_data = {e_id: effect.to_dict() for e_id, effect in effect_skill_data.items()}
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_effect_data, "EffectConfig")

        # Battle Config
        battle_data = {}

        if "BattleConfig" in all_sheets:
            df_battle = all_sheets["BattleConfig"]

            for _, row in df_battle.iterrows():
                battle_id = str(row['ID']).strip()
                if pd.isna(battle_id) or not battle_id:
                    continue

                name_hash=self.get_hash(row['Name'])
                background = str(row['BackGround']).strip() if pd.notna(row['BackGround']) else ""
                reward = str(row['Reward']).strip() if pd.notna(row['Reward']) else ""

                battle_data[battle_id] = BattleModel(
                    name_hash=name_hash,
                    background=background,
                    reward=reward,
                    enemies=[] 
                )

        if "StageEnemies" in all_sheets:
            df_enemies = all_sheets["StageEnemies"]

            for _, row in df_enemies.iterrows():
                battle_id = str(row['BattleID']).strip()

                if battle_id in battle_data:
                    slot = int(row['Slot']) if pd.notna(row['Slot']) else 1
                    enemy_id = str(row['Enemy_ID']).strip() if pd.notna(row['Enemy_ID']) else ""
                    level = int(row['Level']) if pd.notna(row['Level']) else 1

                    enemy_comp = StageEnemiesCompoment(
                        slot=slot,
                        enemy_id=enemy_id,
                        enemy_level=level
                    )

                    battle_data[battle_id].enemies.append(enemy_comp)

        final_battle_data = {b_id: battle.to_dict() for b_id, battle in battle_data.items()}
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_battle_data, "BattleConfig")