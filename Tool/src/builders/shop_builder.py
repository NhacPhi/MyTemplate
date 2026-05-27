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
