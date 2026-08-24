import json, openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

gfd = chars.get('GoldfishDemon', {})
print("=== GoldfishDemon CharacterConfig.json ===")
print(json.dumps(gfd, indent=2))

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx', data_only=True)
for sheetname in ['Character', 'CharacterStat', 'SkillConfig', 'EffectConfig']:
    ws = wb[sheetname]
    print(f"\n=== Sheet: {sheetname} ===")
    headers = [cell.value for cell in ws[1]]
    for row in ws.iter_rows(min_row=2):
        row_vals = [cell.value for cell in row]
        if row_vals[0] and ('Goldfish' in str(row_vals[0]) or 'Goldfish' in str(row_vals)):
            print(dict(zip(headers[:len(row_vals)], row_vals)))
        elif sheetname == 'EffectConfig':
            print(dict(zip(headers[:len(row_vals)], row_vals)))
