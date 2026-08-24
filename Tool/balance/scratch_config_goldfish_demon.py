import json, openpyxl, time, sys
sys.stdout.reconfigure(encoding='utf-8')

# 1. Update EffectConfig.json
with open('Assets/Data/GameConfig/EffectConfig.json', 'r', encoding='utf-8') as f:
    effects = json.load(f)

# Utility String Hash function used by the project
def get_hash(s):
    # FNV-1a or standard hash
    h = 2166136261
    for c in s.encode('utf-8'):
        h = ((h ^ c) * 16777619) & 0xFFFFFFFF
    return h

silence_name_hash = get_hash("EFF_SILENCE_NAME")
silence_des_hash = get_hash("EFF_SILENCE_DES")

effects["EFF_Silence_def"] = {
    "name_hash": silence_name_hash,
    "des_hash": silence_des_hash,
    "type": "Silence",
    "target_stat": "None",
    "modify_type": "None",
    "duration": 1,
    "max_stack": 1,
    "value": 100
}

with open('Assets/Data/GameConfig/EffectConfig.json', 'w', encoding='utf-8') as f:
    json.dump(effects, f, indent=4, ensure_ascii=False)
print("Updated EffectConfig.json with EFF_Silence_def!")

# 2. Update CharacterConfig.json for GoldfishDemon
with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

if "GoldfishDemon" in chars:
    gfd = chars["GoldfishDemon"]
    # Base
    gfd["skills"]["Base"]["damage_multiplier"] = [0.8, 1.0, 1.2]
    gfd["skills"]["Base"]["target_type"] = "SingleEnemy"
    gfd["skills"]["Base"]["effect_id"] = "None"
    
    # Major (Gầm thét sóng âm hàng trước + Câm Lặng 1 lượt)
    gfd["skills"]["Major"]["skill"] = "MajorAttack"
    gfd["skills"]["Major"]["target_type"] = "EnemyRow"
    gfd["skills"]["Major"]["damage_multiplier"] = [0.7, 0.85, 1.0]
    gfd["skills"]["Major"]["max_cooldown"] = [3, 3, 3]
    gfd["skills"]["Major"]["effect_id"] = "EFF_Silence_def"
    
    # Ultimate (Khối nước giáng trùy + Choáng 1 hiệp)
    gfd["skills"]["Ultimate"]["skill"] = "EmpowerAttack"
    gfd["skills"]["Ultimate"]["target_type"] = "SingleEnemy"
    gfd["skills"]["Ultimate"]["damage_multiplier"] = [1.8, 2.0, 2.3]
    gfd["skills"]["Ultimate"]["max_cooldown"] = [4, 4, 4]
    gfd["skills"]["Ultimate"]["effect_id"] = "EFF_Stun_def"
    
    # Stats balancing as Boss 1
    gfd["stats"]["hp"] = 11364
    gfd["stats"]["atk"] = 3350
    gfd["stats"]["def"] = 530
    gfd["stats"]["speed"] = 94
    gfd["stats"]["def_shred"] = 14
    gfd["stats"]["crit_rate"] = 60
    gfd["stats"]["crit_dmg"] = 100
    gfd["stats"]["penetration"] = 20
    gfd["stats"]["crit_dmg_res"] = 25
    
    gfd["upgrades"] = {
        "hp": 568,
        "atk": 168,
        "def": 26
    }
    print("Updated GoldfishDemon skills and stats in CharacterConfig.json!")

with open('Assets/Data/GameConfig/CharacterConfig.json', 'w', encoding='utf-8') as f:
    json.dump(chars, f, indent=4, ensure_ascii=False)

# 3. Update GameConfig.xlsx if accessible
for attempt in range(5):
    try:
        wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
        
        # EffectConfig sheet
        ws_eff = wb['EffectConfig']
        found_eff = False
        for row in ws_eff.iter_rows(min_row=2):
            if row[0].value == 'EFF_Silence_def':
                found_eff = True
                break
        if not found_eff:
            ws_eff.append(['EFF_Silence_def', 'EFF_SILENCE_NAME', 'EFF_SILENCE_DES', 'Silence', 'None', 'None', 1, 100, 1])
            print("Added EFF_Silence_def to EffectConfig sheet in GameConfig.xlsx")
            
        # SkillConfig sheet
        ws_sk = wb['SkillConfig']
        for row in ws_sk.iter_rows(min_row=2):
            s_id = str(row[0].value).strip() if row[0].value else ''
            if s_id == 'GoldfishDemon_B':
                row[4].value = '0.8, 1.0, 1.2'
                row[5].value = '0, 0, 0'
                row[6].value = 'BasicAttack'
                row[7].value = 'SingleEnemy'
                row[8].value = 'None'
            elif s_id == 'GoldfishDemon_M':
                row[3].value = 'MajorAttack'
                row[4].value = '0.7, 0.85, 1.0'
                row[5].value = '3, 3, 3'
                row[6].value = 'ActiveSkill'
                row[7].value = 'EnemyRow'
                row[8].value = 'EFF_Silence_def'
            elif s_id == 'GoldfishDemon_U':
                row[3].value = 'EmpowerAttack'
                row[4].value = '1.8, 2.0, 2.3'
                row[5].value = '4, 4, 4'
                row[6].value = 'ActiveSkill'
                row[7].value = 'SingleEnemy'
                row[8].value = 'EFF_Stun_def'
                
        wb.save('Tool/data/GameConfig.xlsx')
        print("Successfully saved GameConfig.xlsx!")
        break
    except PermissionError:
        print(f"Excel is locked, attempt {attempt+1}, retrying in 1s...")
        time.sleep(1)

# 4. Update Localizations.xlsx
for attempt in range(5):
    try:
        wb_loc = openpyxl.load_workbook('Tool/data/Localizations.xlsx')
        ws_loc = wb_loc['Effect'] if 'Effect' in wb_loc.sheetnames else wb_loc.active
        
        found_name = False
        for row in ws_loc.iter_rows(min_row=2):
            if row[0].value == 'EFF_SILENCE_NAME':
                found_name = True
                break
        if not found_name:
            ws_loc.append(['EFF_SILENCE_NAME', 'Câm Lặng', 'Silence', 'Câm Lặng'])
            ws_loc.append(['EFF_SILENCE_DES', 'Không thể sử dụng Tuyệt kỹ và Kỹ năng', 'Cannot use Major and Ultimate skills', 'Không thể sử dụng Tuyệt kỹ và Kỹ năng'])
            wb_loc.save('Tool/data/Localizations.xlsx')
            print("Successfully updated Localizations.xlsx with Silence strings!")
        break
    except PermissionError:
        print(f"Localizations.xlsx is locked, attempt {attempt+1}, retrying in 1s...")
        time.sleep(1)
