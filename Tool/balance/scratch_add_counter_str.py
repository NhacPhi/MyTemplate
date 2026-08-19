import json
import openpyxl
import sys
import os
sys.stdout.reconfigure(encoding='utf-8')

# 1. Update Localization JSONs
for lang_file, val in [('Assets/Data/Localization/Localization_VIETNAMESE.json', 'Phản Kích'),
                       ('Assets/Data/Localization/Localization_ENGLISH.json', 'Counter-Attack')]:
    if os.path.exists(lang_file):
        with open(lang_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        data['STR_COUNTER_ATTACK'] = val
        # Also hash if needed
        import zlib
        # fnv or simple hash
        def get_hash(text):
            return str(abs(zlib.crc32(text.encode('utf-8'))))
        
        with open(lang_file, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)
        print(f'Added STR_COUNTER_ATTACK to {lang_file}')

# 2. Update Localizations.xlsx
try:
    wb_loc = openpyxl.load_workbook('Tool/data/Localizations.xlsx')
    ws = wb_loc['STR'] if 'STR' in wb_loc else wb_loc.active
    ws.append(['STR_COUNTER_ATTACK', 'Counter-Attack', 'Phản Kích'])
    wb_loc.save('Tool/data/Localizations.xlsx')
    print('Successfully added STR_COUNTER_ATTACK to Localizations.xlsx')
except Exception as e:
    print(f'Could not save Localizations.xlsx directly (file may be open): {e}')
