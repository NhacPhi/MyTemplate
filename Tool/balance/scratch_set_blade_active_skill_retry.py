import openpyxl, time, json, sys
sys.stdout.reconfigure(encoding='utf-8')

# Try updating GameConfig.xlsx with retry
for attempt in range(5):
    try:
        wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
        ws = wb['CombatEvents']
        for row in ws.iter_rows(min_row=2):
            if row[0].value == 'psv_triple_edged_blade':
                row[4].value = 'ActiveSkill, SingleTarget'
                print("Updated psv_triple_edged_blade condition_filter to:", row[4].value)
        wb.save('Tool/data/GameConfig.xlsx')
        print("Successfully saved GameConfig.xlsx!")
        break
    except PermissionError:
        print(f"Attempt {attempt+1}: File locked, retrying in 1s...")
        time.sleep(1)

# Also update PassiveConfig.json directly
with open('Assets/Data/GameConfig/PassiveConfig.json', 'r', encoding='utf-8') as f:
    passives = json.load(f)

if 'psv_triple_edged_blade' in passives:
    for ce in passives['psv_triple_edged_blade'].get('combat_events', []):
        ce['condition_filter'] = 'ActiveSkill, SingleTarget'
    with open('Assets/Data/GameConfig/PassiveConfig.json', 'w', encoding='utf-8') as f:
        json.dump(passives, f, indent=4, ensure_ascii=False)
    print("Successfully updated PassiveConfig.json directly!")
