import pandas as pd
import mmh3
import json
import os
import config
import re

def get_uint32_hash(string_key):
    # MurmurHash3 rturn singed init
    return mmh3.hash(string_key, signed = False)

def clean_variable_name(text):
    """Chuyển ID từ Excel thành tên biến C# hợp lệ"""
    # Thay thế tất cả ký tự không phải chữ và số thành dấu gạch dưới
    clean_name = re.sub(r'[^a-zA-Z0-9_]', '_', text)
    # Xử lý trường hợp ID bắt đầu bằng số (C# không cho phép tên biến bắt đầu bằng số)
    if clean_name[0].isdigit():
        clean_name = "_" + clean_name
    return clean_name.upper()

def export_localization(excel_path):
    # check exit file
    if not os.path.exists(excel_path):
        print(f"Eror: Don't find file: {excel_path}")
        return
    
    # create folder if not exit
    if not os.path.exists(config.OUTPUT_FOLDER):
        os.makedirs(config.OUTPUT_FOLDER)

    print(f"Processing... : {excel_path}")

    try:
        #df = pd.read_excel(excel_path)
        all_sheets = pd.read_excel(excel_path, sheet_name = None)

        loc_vn = {}
        loc_en = {}

        unique_entries = {}

        for sheet_name, df in all_sheets.items():
            print(f'Reading sheet: {sheet_name}')

            # check require cols
            required_cols = ['ID', 'VIETNAMESE', 'ENGLISH']
            if not all(col in df.columns for col in required_cols):
                print(f'Ignore sheet {sheet_name} cause miss ID', 'VIETNAMESE', 'ENGLISH')

            for _, row in df.iterrows():
                key_val = row['ID']
                
                # ignore empty line
                if pd.isna(key_val): continue
                
                key_str = str(key_val).strip()
                if not key_str: continue

                h = get_uint32_hash(key_str)

                loc_vn[h] = str(row['VIETNAMESE']) if pd.notna(row['VIETNAMESE']) else ""
                loc_en[h] = str(row['ENGLISH']) if pd.notna(row['ENGLISH']) else ""

                unique_entries[key_val] = h

         # export to json 
        vn_path = os.path.join(config.OUTPUT_FOLDER, 'Localization_VIETNAMESE.json')
        with open(vn_path, 'w', encoding ='utf-8') as f:
            json.dump(loc_vn, f, indent=4, ensure_ascii=False)

        en_path = os.path.join(config.OUTPUT_FOLDER, 'Localization_ENGLISH.json')
        with open(en_path, 'w', encoding = 'utf-8') as f:
            json.dump(loc_en, f, indent=4, ensure_ascii=False)

        # Export to LocKeys.cs for Unity
        cs_path = os.path.join(config.LOCALIZATION, 'LocKeys.cs')
        with open(cs_path, 'w', encoding='utf-8') as f:
            f.write("//Auto generate\n")
            f.write("public static class LocKeys\n{\n")

            for original_id, hash_val in unique_entries.items():
                var_name = clean_variable_name(original_id)
                f.write(f'    public const long {var_name} = {hash_val};\n\n')
            f.write("}\n")

        print("Gemerated!")
    except Exception as e:
        print(f"Error: {e}")

    