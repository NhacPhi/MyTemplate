import pandas as pd
import config
from src.builders.base_builder import BaseBuilder
from src.models.character_models import CostItem, TierData, AscensionTemplate, ExpCurveConfig

class CharacterUpdateLevelBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing level config: {self.file_path}")

        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

        ascension_templates = {}
        if "AscensionConfig" in all_sheets:
            df_ascension = all_sheets["AscensionConfig"]
            for _, row in df_ascension.iterrows():
                if pd.isna(row['ID']): continue
                template_id = str(row['ID']).strip()
                tier = str(row['Tier'])
                
                if template_id not in ascension_templates:
                    ascension_templates[template_id] = AscensionTemplate()
                
                current_template = ascension_templates[template_id]
                
                current_tier_int = int(row['Tier'])
                if current_tier_int > current_template.max_tier:
                    current_template.max_tier = current_tier_int

                cost_list = []

                coin_cost = int(row['Cost_Coin'])
                if coin_cost > 0:
                    cost_list.append(CostItem(id="Coin", quantity=coin_cost))

                if pd.notna(row['ItemID_01']):
                    item_id = str(row['ItemID_01']).strip()
                    item_qty = int(row['Item_01_Qty'])
                    if item_id != "" and item_qty > 0:
                        cost_list.append(CostItem(id=item_id, quantity=item_qty))
                
                tier_data = TierData(
                    unlock_max_level=int(row['Unlock_Max_Level']),
                    level_req=int(row['Required_Level']),
                    max=1, 
                    cost=cost_list
                )

                current_template.tier[tier] = tier_data
            
        final_data = {item_id: item.to_dict() for item_id, item in ascension_templates.items()}
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_data, "AscensionConfig")

        exp_curves = {}

        if "ExpCurvesConfig" in all_sheets:
            df_levels = all_sheets["ExpCurvesConfig"]

            curve_ids = [col for col in df_levels.columns if col.startswith("Curve_") and not col.endswith("_up")]
        
            for curve_id in curve_ids:
                exp_curves[curve_id] = ExpCurveConfig()

            for _, row in df_levels.iterrows():
                if pd.isna(row['Level']): continue

                level_str = str(int(row['Level']))
                
                for curve_id in curve_ids:
                    if pd.notna(row[curve_id]):
                        exp_curves[curve_id].total_exp[level_str] = int(row[curve_id])
                        
                    up_col_name = f"{curve_id}_up"
                    if up_col_name in df_levels.columns and pd.notna(row[up_col_name]):
                        exp_curves[curve_id].up_exp[level_str] = int(row[up_col_name])

            final_level_data = {curve_id: curve.to_dict() for curve_id, curve in exp_curves.items()}
            self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_level_data, "ExpCurvesConfig")
