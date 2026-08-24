import json, openpyxl, time, sys
sys.stdout.reconfigure(encoding='utf-8')

# 1. Update CharacterConfig.json
with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

if "ThirdDragonPrince" in chars:
    tdp = chars["ThirdDragonPrince"]
    
    # Base Skill
    tdp["skills"]["Base"]["skill"] = "Melee"
    tdp["skills"]["Base"]["target_type"] = "SingleEnemy"
    tdp["skills"]["Base"]["damage_multiplier"] = [0.9, 1.0, 1.2]
    tdp["skills"]["Base"]["max_cooldown"] = [0, 0, 0]
    tdp["skills"]["Base"]["effect_id"] = "None"
    
    # Major Skill
    tdp["skills"]["Major"]["skill"] = "FrozenBall"
    tdp["skills"]["Major"]["target_type"] = "SingleEnemy"
    tdp["skills"]["Major"]["damage_multiplier"] = [1.0, 1.2, 1.4]
    tdp["skills"]["Major"]["max_cooldown"] = [3, 3, 3]
    tdp["skills"]["Major"]["effect_id"] = "EFF_DefSpd_def"
    tdp["skills"]["Major"]["passive_id"] = "psv_thirddragon_major"
    
    # Ultimate Skill (AllEnemies + EFF_Frozen_def)
    tdp["skills"]["Ultimate"]["skill"] = "EmpowerAttack"
    tdp["skills"]["Ultimate"]["target_type"] = "AllEnemies"
    tdp["skills"]["Ultimate"]["damage_multiplier"] = [1.4, 1.6, 1.8]
    tdp["skills"]["Ultimate"]["max_cooldown"] = [4, 4, 4]
    tdp["skills"]["Ultimate"]["effect_id"] = "EFF_Frozen_def"
    tdp["skills"]["Ultimate"]["passive_id"] = ""
    
    # Stats (Balanced for Level 20 World 1 Climax Boss)
    tdp["stats"]["hp"] = 14500
    tdp["stats"]["atk"] = 3600
    tdp["stats"]["def"] = 580
    tdp["stats"]["speed"] = 98
    tdp["stats"]["def_shred"] = 15
    tdp["stats"]["crit_rate"] = 60
    tdp["stats"]["crit_dmg"] = 100
    tdp["stats"]["penetration"] = 20
    tdp["stats"]["crit_dmg_res"] = 25
    
    tdp["upgrades"] = {
        "hp": 725,
        "atk": 180,
        "def": 29
    }
    
    tdp["attributes"]["shield"] = {
        "max_stat_type": "HP",
        "start_percent": 0.2
    }
    print("Updated ThirdDragonPrince in CharacterConfig.json!")

with open('Assets/Data/GameConfig/CharacterConfig.json', 'w', encoding='utf-8') as f:
    json.dump(chars, f, indent=4, ensure_ascii=False)

# 2. Update PassiveConfig.json to add psv_thirddragon_major
with open('Assets/Data/GameConfig/PassiveConfig.json', 'r', encoding='utf-8') as f:
    passives = json.load(f)

passives["psv_thirddragon_major"] = {
    "desc_template_hash": 1393091217,
    "static_modifiers": [
        {
            "stat_type": "ATK",
            "modify_type": "Percent",
            "modify_by_upgrade": [
                10.0,
                15.0,
                20.0,
                25.0,
                30.0,
                35.0
            ]
        }
    ],
    "combat_events": [
        {
            "event_type": "OnAfterDealDamage",
            "effect_id": "EFF_DefSpd_def",
            "modify_by_upgrade": [
                -20.0,
                -25.0,
                -30.0,
                -35.0,
                -40.0,
                -50.0
            ],
            "target": "target",
            "condition_filter": "MajorSkill",
            "effect_param": 0.0,
            "internal_cooldown": 0
        }
    ]
}

with open('Assets/Data/GameConfig/PassiveConfig.json', 'w', encoding='utf-8') as f:
    json.dump(passives, f, indent=4, ensure_ascii=False)
print("Updated PassiveConfig.json with psv_thirddragon_major!")

# 3. Update GameConfig.xlsx
for attempt in range(10):
    try:
        wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
        
        # A. SkillConfig sheet
        ws_sk = wb['SkillConfig']
        sk_headers = [c.value for c in ws_sk[1]]
        col_type = sk_headers.index('Type') if 'Type' in sk_headers else 3
        col_dmg = sk_headers.index('DamageMultiplier') if 'DamageMultiplier' in sk_headers else 4
        col_cd = sk_headers.index('MaxCooldown') if 'MaxCooldown' in sk_headers else 5
        col_stype = sk_headers.index('SkillType') if 'SkillType' in sk_headers else 6
        col_ttype = sk_headers.index('TargetType') if 'TargetType' in sk_headers else 7
        col_eff = sk_headers.index('Effect') if 'Effect' in sk_headers else 8
        col_pas = sk_headers.index('PassiveID') if 'PassiveID' in sk_headers else 10
        
        for row in ws_sk.iter_rows(min_row=2):
            s_id = str(row[0].value).strip() if row[0].value else ''
            if s_id == 'ThirdDragonPrince_B':
                row[col_dmg].value = '0.9, 1.0, 1.2'
                row[col_cd].value = '0, 0, 0'
                row[col_stype].value = 'BasicAttack'
                row[col_ttype].value = 'SingleEnemy'
                row[col_eff].value = 'None'
                row[col_pas].value = 'None'
            elif s_id == 'ThirdDragonPrince_M':
                row[col_type].value = 'FrozenBall'
                row[col_dmg].value = '1.0, 1.2, 1.4'
                row[col_cd].value = '3, 3, 3'
                row[col_stype].value = 'ActiveSkill'
                row[col_ttype].value = 'SingleEnemy'
                row[col_eff].value = 'EFF_DefSpd_def'
                row[col_pas].value = 'psv_thirddragon_major'
            elif s_id == 'ThirdDragonPrince_U':
                row[col_type].value = 'EmpowerAttack'
                row[col_dmg].value = '1.4, 1.6, 1.8'
                row[col_cd].value = '4, 4, 4'
                row[col_stype].value = 'ActiveSkill'
                row[col_ttype].value = 'AllEnemies'
                row[col_eff].value = 'EFF_Frozen_def'
                row[col_pas].value = 'None'
                
        # B. CharacterStat sheet
        ws_stat = wb['CharacterStat']
        stat_headers = [c.value for c in ws_stat[1]]
        for row in ws_stat.iter_rows(min_row=2):
            c_id = str(row[0].value).strip() if row[0].value else ''
            if c_id == 'ThirdDragonPrince':
                row[stat_headers.index('hp')].value = 14500
                row[stat_headers.index('atk')].value = 3600
                row[stat_headers.index('def')].value = 580
                row[stat_headers.index('speed')].value = 98
                row[stat_headers.index('def_shred')].value = 15
                row[stat_headers.index('crit_rare')].value = 60
                row[stat_headers.index('crit_dmg')].value = 100
                row[stat_headers.index('penetration')].value = 20
                row[stat_headers.index('crit_dmg_res')].value = 25
                
        # C. CharacterUpgrade sheet
        ws_up = wb['CharacterUpgrade']
        for row in ws_up.iter_rows(min_row=2):
            c_id = str(row[0].value).strip() if row[0].value else ''
            if c_id == 'ThirdDragonPrince':
                row[1].value = 725
                row[2].value = 180
                row[3].value = 29
                
        # D. Passives sheet
        ws_pas = wb['Passives']
        pas_keys = {row[0].value for row in ws_pas.iter_rows(min_row=2) if row[0].value}
        if 'psv_thirddragon_major' not in pas_keys:
            ws_pas.append(['psv_thirddragon_major', 'STR_THIRD_DRAGON_MAJOR_PAS', 'Bắn cầu băng gây sát thương và làm Tê Tái giảm {0}% Tốc độ trong 2 hiệp.'])
            
        # E. StaticModifiers sheet
        ws_mod = wb['StaticModifiers']
        mod_keys = {row[0].value for row in ws_mod.iter_rows(min_row=2) if row[0].value}
        if 'psv_thirddragon_major' not in mod_keys:
            ws_mod.append(['psv_thirddragon_major', 'ATK', 'Percent', '10, 15, 20, 25, 30, 35', 'Tăng {0}% ATK'])
            
        # F. CombatEvents sheet
        ws_ce = wb['CombatEvents']
        ce_keys = {row[0].value for row in ws_ce.iter_rows(min_row=2) if row[0].value}
        if 'psv_thirddragon_major' not in ce_keys:
            ws_ce.append(['psv_thirddragon_major', 'OnAfterDealDamage', 'EFF_DefSpd_def', '-20, -25, -30, -35, -40, -50', 'MajorSkill', 0, None, 'target', 'Làm giảm tốc độ kẻ địch'])
            
        wb.save('Tool/data/GameConfig.xlsx')
        print("Successfully saved GameConfig.xlsx with ThirdDragonPrince updates!")
        break
    except PermissionError:
        print(f"GameConfig.xlsx locked (attempt {attempt+1}/10), retrying in 1s...")
        time.sleep(1)
