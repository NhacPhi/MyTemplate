import pandas as pd
import config
from src.builders.base_builder import BaseBuilder

class StarUpBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing StarUp config: {self.file_path}")

        try:
            all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        except Exception as e:
            print(f"Failed to read {self.file_path}: {e}")
            return

        starup_configs = {}
        
        if "StarUp" in all_sheets:
            df = all_sheets["StarUp"]
            for _, row in df.iterrows():
                if pd.isna(row['ID']): continue
                
                s_id = str(row['ID']).strip()
                tier = str(int(row['Tier']))
                
                coin = int(row['Cost_Coin']) if pd.notna(row['Cost_Coin']) else 0
                qty = int(row['Quanlity']) if pd.notna(row['Quanlity']) else 0
                
                if s_id not in starup_configs:
                    starup_configs[s_id] = {
                        "max_tier": 0,
                        "tiers": {}
                    }
                    
                cfg = starup_configs[s_id]
                tier_int = int(tier)
                
                if tier_int > cfg["max_tier"]:
                    cfg["max_tier"] = tier_int
                    
                cfg["tiers"][tier] = {
                    "cost_coin": coin,
                    "quantity": qty
                }
                
            self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, starup_configs, "StarUpConfig")
            print("Successfully exported StarUpConfig.json")
        else:
            print("Sheet 'StarUp' not found in the excel file.")
