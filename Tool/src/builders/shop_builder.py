import pandas as pd
import config
from src.builders.base_builder import BaseBuilder

class ShopConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing shop config: {self.file_path}")
        
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        
        master_data = {}
        
        if "ShopProducts" in all_sheets:
            df = all_sheets["ShopProducts"]
            for _, row in df.iterrows():
                if pd.isna(row['ProductID']): continue
                product_id = str(row['ProductID']).strip()
                master_data[product_id] = {
                    "product_id": product_id,
                    "shop_category": str(row['ShopCategory']).strip() if pd.notna(row['ShopCategory']) else "",
                    "sub_category": str(row['SubCategory']).strip() if pd.notna(row['SubCategory']) else "",
                    "sell_type": str(row['SellType']).strip() if pd.notna(row['SellType']) else "",
                    "reference_id": str(row['ReferenceID']).strip() if pd.notna(row['ReferenceID']) else "",
                    "item_amount": int(row['ItemAmount']) if pd.notna(row['ItemAmount']) else 0,
                    "currency_type": str(row['CurrencyType']).strip() if pd.notna(row['CurrencyType']) else "",
                    "price": float(row['Price']) if pd.notna(row['Price']) else 0.0,
                    "original_price": float(row['OriginalPrice']) if pd.notna(row['OriginalPrice']) else 0.0,
                    "limit_count": int(row['LimitCount']) if pd.notna(row['LimitCount']) else 0,
                    "limit_type": str(row['LimitType']).strip() if pd.notna(row['LimitType']) else "",
                    "start_time": str(row['StartTime']).strip() if pd.notna(row['StartTime']) else "",
                    "end_time": str(row['EndTime']).strip() if pd.notna(row['EndTime']) else "",
                    "is_active": bool(row['IsActive']) if pd.notna(row['IsActive']) else True,
                    "sort_order": int(row['SortOrder']) if pd.notna(row['SortOrder']) else 0,
                    "bundle_contents": []
                }
                
        if "BundleContents" in all_sheets:
            df_bundle = all_sheets["BundleContents"]
            for _, row in df_bundle.iterrows():
                if pd.isna(row['BundleID']): continue
                bundle_id = str(row['BundleID']).strip()
                item_id = str(row['ItemID']).strip() if pd.notna(row['ItemID']) else ""
                amount = int(row['Amount']) if pd.notna(row['Amount']) else 0
                
                # Assign bundle contents to products that reference this bundle
                for p_id, product in master_data.items():
                    if product["sell_type"] == "Bundle" and product["reference_id"] == bundle_id:
                        product["bundle_contents"].append({
                            "item_id": item_id,
                            "amount": amount
                        })
                        
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, master_data, "ShopConfig")

        # Parse and export SevenDayLogin rewards sheet if present
        if "SevenDayLogin" in all_sheets:
            df_seven = all_sheets["SevenDayLogin"]
            seven_day_data = []
            for _, row in df_seven.iterrows():
                if pd.isna(row['Day']): continue
                day_num = int(row['Day'])
                
                custom_name = ""
                if 'CustomName' in row and pd.notna(row['CustomName']):
                    custom_name = str(row['CustomName']).strip()
                elif 'Name' in row and pd.notna(row['Name']):
                    custom_name = str(row['Name']).strip()

                seven_day_data.append({
                    "day_number": day_num,
                    "reward_type": str(row['Type']).strip() if pd.notna(row['Type']) else "Item",
                    "reward_id": str(row['RewardID']).strip() if pd.notna(row['RewardID']) else "",
                    "amount": int(row['Amount']) if pd.notna(row['Amount']) else 1,
                    "custom_name": custom_name
                })
            self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, seven_day_data, "SevenDayLoginConfig")
            print(f"Successfully exported SevenDayLoginConfig.json with {len(seven_day_data)} days.")

        # Parse and export RedeemCode sheet if present
        if "RedeemCode" in all_sheets:
            df_redeem = all_sheets["RedeemCode"]
            redeem_dict = {}
            for _, row in df_redeem.iterrows():
                if pd.isna(row.get('Code')): continue
                code = str(row['Code']).strip().upper()
                if not code: continue

                if code not in redeem_dict:
                    redeem_dict[code] = {
                        "code": code,
                        "is_active": bool(row['IsActive']) if pd.notna(row.get('IsActive')) else True,
                        "rewards": []
                    }

                reward_type = str(row['Type']).strip() if pd.notna(row.get('Type')) else "Item"
                reward_id = str(row['RewardID']).strip() if pd.notna(row.get('RewardID')) else ""
                amount = int(row['Amount']) if pd.notna(row.get('Amount')) else 1

                redeem_dict[code]["rewards"].append({
                    "type": reward_type,
                    "id": reward_id,
                    "amount": amount
                })

            self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, redeem_dict, "RedeemCodeConfig")
            print(f"Successfully exported RedeemCodeConfig.json with {len(redeem_dict)} codes.")

