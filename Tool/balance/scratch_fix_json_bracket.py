import json
import openpyxl
import os
import sys
sys.stdout.reconfigure(encoding='utf-8')

# 1. Fix in Localization JSONs
for lang_file in ['Assets/Data/Localization/Localization_VIETNAMESE.json', 'Assets/Data/Localization/Localization_ENGLISH.json']:
    if os.path.exists(lang_file):
        with open(lang_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        modified = False
        for k, v in data.items():
            if isinstance(v, str) and '{1]' in v:
                data[k] = v.replace('{1]', '{1}')
                print(f'Fixed {{1] in {lang_file} key {k}')
                modified = True
        
        if modified:
            with open(lang_file, 'w', encoding='utf-8') as f:
                json.dump(data, f, indent=2, ensure_ascii=False)
            print(f'Saved {lang_file}')

# 2. Fix in Localizations.xlsx
try:
    wb_loc = openpyxl.load_workbook('Tool/data/Localizations.xlsx')
    for ws in wb_loc.worksheets:
        for row in ws.iter_rows(min_row=2):
            for cell in row:
                if cell.value and isinstance(cell.value, str) and '{1]' in cell.value:
                    cell.value = cell.value.replace('{1]', '{1}')
                    print(f'Fixed {{1] in sheet [{ws.title}] row {row[0].value}')
    wb_loc.save('Tool/data/Localizations.xlsx')
    print('Successfully saved Localizations.xlsx')
except Exception as e:
    print(f'Could not save Localizations.xlsx directly: {e}')
