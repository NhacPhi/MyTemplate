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
        clean_name = re.sub(r'[^a-zA-Z0-9_]', '_', str(text).strip())
        if clean_name and clean_name[0].isdigit():
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
                s_name = sheet_name.strip()
                print(f'Reading sheet: {s_name}')

                # Chuẩn hóa tên cột
                col_map = {}
                for col in df.columns:
                    c_clean = str(col).strip().upper()
                    col_map[c_clean] = col

                # Tìm cột ID (hỗ trợ ID, ST, KEY)
                id_col = None
                for candidate in ['ID', 'ST', 'KEY']:
                    if candidate in col_map:
                        id_col = col_map[candidate]
                        break

                vn_col = col_map.get('VIETNAMESE')
                en_col = col_map.get('ENGLISH')

                if not id_col or not vn_col or not en_col:
                    print(f'Ignore sheet {s_name} cause missing ID/ST, VIETNAMESE, or ENGLISH (Columns: {list(df.columns)})')
                    continue

                sheet_count = 0
                for _, row in df.iterrows():
                    key_val = row[id_col]
                    if pd.isna(key_val): continue
                    
                    key_str = str(key_val).strip()
                    if not key_str: continue

                    h = self.get_hash(key_str)

                    loc_vn[h] = str(row[vn_col]) if pd.notna(row[vn_col]) else ""
                    loc_en[h] = str(row[en_col]) if pd.notna(row[en_col]) else ""

                    unique_entries[key_str] = h
                    sheet_count += 1

                print(f'  -> Sheet {s_name}: loaded {sheet_count} entries.')

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

            seen_var_names = set()
            with open(cs_path, 'w', encoding='utf-8') as f:
                f.write("//Auto generate\n")
                f.write("public static class LocKeys\n{\n")

                for original_id, hash_val in unique_entries.items():
                    var_name = self.clean_variable_name(original_id)
                    if not var_name or var_name in seen_var_names:
                        continue
                    seen_var_names.add(var_name)
                    f.write(f'    public const long {var_name} = {hash_val};\n\n')
                f.write("}\n")

            print(f"Generated! Total unique keys: {len(unique_entries)}, Total C# constants: {len(seen_var_names)}")
        except Exception as e:
            print(f"Error: {e}")
