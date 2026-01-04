import pandas as pd
import mmh3
import json
import os
import re

import config
from src.models import ItemModel, WeaponComponent, ExpComponent, ArmorComponent, CharacterModel

class BaseBuilder:
    def get_hash(self, key):
        if pd.isna(key) or str(key).strip() == "": return 0
        return mmh3.hash(str(key).strip(), signed=False)
    def export_json(self, data, filename):
        path = os.path.join(config.OUTPUT_FOLDER, f"{filename}.json")
        with open(path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=4, ensure_ascii=False)

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
        self.export_json(final_data, "ItemConfig")

class CharacterConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path
    def run(self):
        print(f"Processing character config: {self.file_path}")
        
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        character_data = {}
        # CharacterConfig
        if "Character" in all_sheets:
            df = all_sheets["Character"]
            for _, row, in df.iterrows():
                if pd.isna(row['ID']) : continue

                character_id = str(row['ID']).strip()

                character_data[character_id] = CharacterModel(
                    name_hash=self.get_hash(row['Name']),
                    rare=str(row['Rare']),
                    type=str(row['Type']),
                )

        # Handle character stats
        if "CharacterStat" in all_sheets:
            df = all_sheets["CharacterStat"]

            # defind clomums
            stat_columns = ['hp', 'def', 'atk']
            
            for _, row, in df.iterrows():
                char_id = str(row['ID']).strip()
                if char_id in character_data:
                    char_stats = {}
                    for col in stat_columns:
                        if col in df.columns:
                            char_stats[col] = int(row[col]) if not pd.isna(row[col]) else 0
                    character_data[char_id].stats = char_stats

    
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
        self.export_json(final_data, "CharacterConfig")