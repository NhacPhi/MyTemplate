import pandas as pd
import config
from src.builders.base_builder import BaseBuilder

class RewardConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing reward config: {self.file_path}")
        
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        
        master_data = {}
        
        if "RewardConfig" in all_sheets:
            df = all_sheets["RewardConfig"]
            for _, row in df.iterrows():
                if pd.isna(row['reward_id']): continue
                reward_id = str(row['reward_id']).strip()
                
                rewards_list = []
                # Quét qua 4 cột item_01/amount_01 đến item_04/amount_04
                for i in range(1, 5):
                    item_col = f"item_{i:02d}"
                    amount_col = f"amount_{i:02d}"
                    
                    # Kiểm tra xem cột có tồn tại không và có giá trị không
                    if item_col in row and amount_col in row:
                        if pd.notna(row[item_col]) and str(row[item_col]).strip() != "":
                            item_id = str(row[item_col]).strip()
                            amount = int(row[amount_col]) if pd.notna(row[amount_col]) else 1
                            rewards_list.append({
                                "item_id": item_id,
                                "amount": amount
                            })
                            
                master_data[reward_id] = {
                    "reward_id": reward_id,
                    "rewards": rewards_list
                }
                
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, master_data, "RewardConfig")
