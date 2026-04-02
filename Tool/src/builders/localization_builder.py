import pandas as pd
import json
import os
import re
import config
from src.builders.base_builder import BaseBuilder

class LocalizationBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def clean_variable_name(self, text):
        """Chuyển ID từ Excel thành tên biến C# hợp lệ"""
        clean_name = re.sub(r'[^a-zA-Z0-9_]', '_', str(text))
        if clean_name[0].isdigit():
            clean_name = "_" + clean_name
        return clean_name.upper()

    def run(self):
        excel_path = self.file_path
        if not os.path.exists(excel_path):
            print(f"Error: Don't find file: {excel_path}")
            return
        
        if not os.path.exists(config.OUTPUT_GAME_LOCALIZATION_FOLDER):
            os.makedirs(config.OUTPUT_GAME_LOCALIZATION_FOLDER)

        print(f"Processing... : {excel_path}")

        try:
            all_sheets = pd.read_excel(excel_path, sheet_name=None)
            loc_vn = {}
            loc_en = {}
            unique_entries = {}

            for sheet_name, df in all_sheets.items():
                print(f'Reading sheet: {sheet_name}')

                required_cols = ['ID', 'VIETNAMESE', 'ENGLISH']
                if not all(col in df.columns for col in required_cols):
                    print(f'Ignore sheet {sheet_name} cause miss ID, VIETNAMESE, ENGLISH')
                    continue

                for _, row in df.iterrows():
                    key_val = row['ID']
                    if pd.isna(key_val): continue
                    
                    key_str = str(key_val).strip()
                    if not key_str: continue

                    h = self.get_hash(key_str)

                    loc_vn[h] = str(row['VIETNAMESE']) if pd.notna(row['VIETNAMESE']) else ""
                    loc_en[h] = str(row['ENGLISH']) if pd.notna(row['ENGLISH']) else ""

                    unique_entries[key_str] = h

            vn_path = os.path.join(config.OUTPUT_GAME_LOCALIZATION_FOLDER, 'Localization_VIETNAMESE.json')
            with open(vn_path, 'w', encoding='utf-8') as f:
                json.dump(loc_vn, f, indent=4, ensure_ascii=False)

            en_path = os.path.join(config.OUTPUT_GAME_LOCALIZATION_FOLDER, 'Localization_ENGLISH.json')
            with open(en_path, 'w', encoding='utf-8') as f:
                json.dump(loc_en, f, indent=4, ensure_ascii=False)

            cs_path = os.path.join(config.LOCALIZATION, 'LocKeys.cs')
            cs_dir = os.path.dirname(cs_path)
            if not os.path.exists(cs_dir):
                os.makedirs(cs_dir, exist_ok=True)

            with open(cs_path, 'w', encoding='utf-8') as f:
                f.write("//Auto generate\n")
                f.write("public static class LocKeys\n{\n")

                for original_id, hash_val in unique_entries.items():
                    var_name = self.clean_variable_name(original_id)
                    f.write(f'    public const long {var_name} = {hash_val};\n\n')
                f.write("}\n")

            print("Generated!")
        except Exception as e:
            print(f"Error: {e}")
