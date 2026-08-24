import openpyxl, time, sys
sys.stdout.reconfigure(encoding='utf-8')

for attempt in range(10):
    try:
        wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
        ws = wb['CombatEvents']
        for row in ws.iter_rows(min_row=2):
            if row[0].value == 'psv_triple_edged_blade':
                row[4].value = 'ActiveSkill, SingleTarget, TargetDead'
                print("Updated GameConfig.xlsx psv_triple_edged_blade condition_filter to:", row[4].value)
        wb.save('Tool/data/GameConfig.xlsx')
        print("Successfully saved GameConfig.xlsx!")
        break
    except PermissionError:
        print(f"Excel is locked, attempt {attempt+1}, retrying in 1s...")
        time.sleep(1)
