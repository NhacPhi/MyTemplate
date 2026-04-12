import pandas as pd
import config
from src.builders.base_builder import BaseBuilder
from src.models.passive_skill_models import PassiveSkillModel

class PassiveSkillBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing PassiveSkill config: {self.file_path}")

        try:
            all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        except Exception as e:
            print(f"Failed to read {self.file_path}: {e}")
            return

        required_sheets = ["Passives", "StaticModifiers", "CombatEvents"]
        for sheet in required_sheets:
            if sheet not in all_sheets:
                print(f"[Error] Missing required sheet: '{sheet}' in {self.file_path}")
                return

        raw_passives_dict = {}

        # --- BƯỚC 1: Passives ---
        df_passives = all_sheets["Passives"]
        for _, row in df_passives.iterrows():
            if pd.isna(row.get('ID')): 
                continue
                
            skill_id = str(row['ID']).strip()
            desc_template_hash = self.get_hash(str(row['desc_template'])) if pd.notna(row.get('desc_template')) else 0
            
            raw_passives_dict[skill_id] = {
                "id": skill_id, # Vẫn truyền ID vào Model để khởi tạo
                "desc_template_hash": desc_template_hash,
                "static_modifiers": [],
                "combat_events": []
            }

        # --- BƯỚC 2: StaticModifiers ---
        df_static = all_sheets["StaticModifiers"]
        for _, row in df_static.iterrows():
            if pd.isna(row.get('passive_id')):
                continue
                
            pid = str(row['passive_id']).strip()
            if pid in raw_passives_dict:
                mod_data = {
                    "stat_type": str(row['stat_type']).strip() if pd.notna(row.get('stat_type')) else "",
                    "modify_type": str(row['modify_type']).strip() if pd.notna(row.get('modify_type')) else "",
                    "modify_by_upgrade": row.get('modify_by_upgrade', "")
                }
                raw_passives_dict[pid]["static_modifiers"].append(mod_data)

        # --- BƯỚC 3: CombatEvents ---
        df_events = all_sheets["CombatEvents"]
        for _, row in df_events.iterrows():
            if pd.isna(row.get('passive_id')):
                continue
                
            pid = str(row['passive_id']).strip()
            if pid in raw_passives_dict:
                event_data = {
                    "event_type": str(row['event_type']).strip() if pd.notna(row.get('event_type')) else "",
                    "effect_id": str(row['effect_id']).strip() if pd.notna(row.get('effect_id')) else "",
                    "modify_by_upgrade": row.get('modify_by_upgrade', ""),
                    "condition_filter": str(row['condition_filter']).strip() if pd.notna(row.get('condition_filter')) else "",
                    "effect_param": float(row['effect_param']) if pd.notna(row.get('effect_param')) else 0.0,
                    "internal_cooldown": int(row['internal_cooldown']) if pd.notna(row.get('internal_cooldown')) else 0
                }
                raw_passives_dict[pid]["combat_events"].append(event_data)

        # --- BƯỚC 4: Xuất JSON ---
        final_models = {}
        for pid, raw_data in raw_passives_dict.items():
            try:
                final_models[pid] = PassiveSkillModel(**raw_data)
            except Exception as e:
                print(f"[Warning] Failed to parse model for {pid}: {e}")

        # Khi gọi item.to_dict(), nó sẽ tự động ẩn key 'id' đi nhờ logic mới
        final_json_data = {item_id: item.to_dict() for item_id, item in final_models.items()}
        
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_json_data, "PassiveConfig")
        print(f"Successfully exported PassiveConfig.json with {len(final_models)} skills.")