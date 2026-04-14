import pandas as pd
import config
from src.builders.base_builder import BaseBuilder
from src.models.item_models import ItemModel, WeaponComponent, ExpComponent, ArmorComponent, MainStatData

class ItemConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing item config: {self.file_path}")
        
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

        master_data = {}
        if "Item_Master" in all_sheets:
            df = all_sheets["Item_Master"]
            for _, row in df.iterrows():
                if pd.isna(row['ID']) : continue

                item_id = str(row['ID']).strip()

                master_data[item_id] = ItemModel(
                    name_hash=self.get_hash(row['Name']),
                    description_hash=self.get_hash(row['Description']),
                    use_hash=self.get_hash(row['UseDescription']),
                    item_type=str(row['ItemType']),
                    rarity=str(row['Rarity'])
                )

        if "Weapon" in all_sheets:
            df_weapon = all_sheets["Weapon"]
            for _, row in df_weapon.iterrows():
                w_id = str(row['ID']).strip()
                if w_id in master_data and master_data[w_id].item_type == "Weapon":
                    master_data[w_id].weapon_data = WeaponComponent(
                        weapon_type= str(row['Type']) if pd.notna(row['Type']) else "None",
                        passive_id= str(row['PassiveID']) if pd.notna(row['PassiveID']) else "",
                        stats={
                            "hp": int(row['HP']) if pd.notna(row['HP']) else 0,
                            "atk": int(row['ATK']) if pd.notna(row['ATK']) else 0,
                        },
                        upgrades={
                            "hp": int(row['GrowthHP']) if pd.notna(row['GrowthHP']) else 0,
                            "atk": int(row['GrowthATK']) if pd.notna(row['GrowthATK']) else 0,
                        }
                    )

        if "Armor" in all_sheets:
            df_armor = all_sheets["Armor"]
            for _, row in df_armor.iterrows():
                w_id = str(row['ID']).strip()
                
                if w_id in master_data and master_data[w_id].item_type == "Armor":
                    m_type = str(row['main_stat_type']).strip() if pd.notna(row['main_stat_type']) else ""
                    m_value = float(row['main_stat_valuie']) if pd.notna(row['main_stat_valuie']) else 0.0
                    m_modifier = str(row['modifier_type']) if pd.notna(row['modifier_type']) else ""

                    main_stat_obj = MainStatData(type=m_type, value=m_value, mod_type=m_modifier) if m_type else None
          
                    master_data[w_id].armor_data = ArmorComponent(
                        part=str(row['Part']).strip() if pd.notna(row['Part']) else "",
                        armor_set=str(row['SetArmor']).strip() if pd.notna(row['SetArmor']) else "",
                        main_stat=main_stat_obj,
                        substat_pool_id=str(row['substat_pool_id']) if pd.notna(row['substat_pool_id']) else ""
                    )                

        if "Item_Detail" in all_sheets:
            df_item_detail = all_sheets["Item_Detail"]
            for _, row in df_item_detail.iterrows():
                w_id = str(row['ID']).strip()
                if w_id in master_data and master_data[w_id].item_type == "Exp":
                    master_data[w_id].exp_data = ExpComponent(
                        value=int(row['Pram01']) if pd.notna(row['Pram01']) else 0
                    )

        final_data = {item_id: item.to_dict() for item_id, item in master_data.items()}
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_data, "ItemConfig")
