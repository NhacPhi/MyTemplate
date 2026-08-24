import json, openpyxl, time, sys
sys.stdout.reconfigure(encoding='utf-8')

# 1. Update CharacterConfig.json
with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

if "ThirdDragonPrince" in chars:
    tdp_u = chars["ThirdDragonPrince"]["skills"]["Ultimate"]
    tdp_u["target_type"] = "SingleEnemy"
    tdp_u["damage_multiplier"] = [1.8, 2.0, 2.3]
    print("Updated ThirdDragonPrince_U to SingleEnemy in CharacterConfig.json!")

with open('Assets/Data/GameConfig/CharacterConfig.json', 'w', encoding='utf-8') as f:
    json.dump(chars, f, indent=4, ensure_ascii=False)

# 2. Update GameConfig.xlsx
for attempt in range(10):
    try:
        wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
        ws_sk = wb['SkillConfig']
        sk_headers = [c.value for c in ws_sk[1]]
        col_dmg = sk_headers.index('DamageMultiplier') if 'DamageMultiplier' in sk_headers else 4
        col_ttype = sk_headers.index('TargetType') if 'TargetType' in sk_headers else 7
        
        for row in ws_sk.iter_rows(min_row=2):
            s_id = str(row[0].value).strip() if row[0].value else ''
            if s_id == 'ThirdDragonPrince_U':
                row[col_dmg].value = '1.8, 2.0, 2.3'
                row[col_ttype].value = 'SingleEnemy'
                print("Updated ThirdDragonPrince_U to SingleEnemy in GameConfig.xlsx!")
                
        wb.save('Tool/data/GameConfig.xlsx')
        print("Successfully saved GameConfig.xlsx!")
        break
    except PermissionError:
        print(f"GameConfig.xlsx locked (attempt {attempt+1}), retrying in 1s...")
        time.sleep(1)
