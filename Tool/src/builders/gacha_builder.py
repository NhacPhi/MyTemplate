import pandas as pd
import config
from src.builders.base_builder import BaseBuilder

class GachaConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing gacha config: {self.file_path}")
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        
        gacha_data = {}

        if "Banners" in all_sheets:
            df_banners = all_sheets["Banners"]
            for _, row in df_banners.iterrows():
                if pd.isna(row['BannerID']): continue
                banner_id = str(row['BannerID']).strip()
                gacha_data[banner_id] = {
                    "bannerId": banner_id,
                    "name_hash": self.get_hash(row['Name']),
                    "type": str(row['BannerType']),
                    "pityLimit": int(row['PityLimit']) if pd.notna(row['PityLimit']) else 40,
                    "cost": {
                        "type": str(row['CostType']),
                        "amount": int(row['CostAmount']) if pd.notna(row['CostAmount']) else 1
                    },
                    "allowSelection": bool(row['AllowSelection']) if pd.notna(row['AllowSelection']) else False,
                    "rates": {},
                    "pool": []
                }

        if "Rates" in all_sheets:
            df_rates = all_sheets["Rates"]
            for _, row in df_rates.iterrows():
                if pd.isna(row['BannerID']): continue
                banner_id = str(row['BannerID']).strip()
                if banner_id in gacha_data:
                    rarity = str(int(row['Rarity'])) if pd.notna(row['Rarity']) else "0"
                    gacha_data[banner_id]["rates"][rarity] = {
                        "baseRate": float(row['BaseRate']) if pd.notna(row['BaseRate']) else 0.0,
                        "isGuarantee": bool(row['PityGuarantee']) if pd.notna(row['PityGuarantee']) else False
                    }

        if "Pools" in all_sheets:
            df_pools = all_sheets["Pools"]
            for _, row in df_pools.iterrows():
                if pd.isna(row['BannerID']): continue
                banner_id = str(row['BannerID']).strip()
                if banner_id in gacha_data:
                    pool_item = {
                        "itemId": str(row['ItemID']).strip(),
                        "rarity": int(row['Rarity']) if pd.notna(row['Rarity']) else 3,
                        "isRateUp": bool(row['IsRateUp']) if pd.notna(row['IsRateUp']) else False,
                        "isSelectableTarget": bool(row['IsSelectableTarget']) if pd.notna(row['IsSelectableTarget']) else False,
                        "weight": int(row['Weight']) if pd.notna(row['Weight']) else 100
                    }
                    gacha_data[banner_id]["pool"].append(pool_item)

        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, gacha_data, "GachaConfig")
