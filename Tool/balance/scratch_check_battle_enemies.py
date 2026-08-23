import openpyxl, json, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx', data_only=True)
ws_stage = wb['StageEnemies']
ws_char = wb['Character']

stage_enemies = {}
for r in ws_stage.iter_rows(min_row=2, values_only=True):
    if r[0] and r[2]:
        b_id = r[0]
        e_id = str(r[2]).strip()
        is_boss = bool(r[4])
        stage_enemies[e_id] = is_boss

print("All Enemies in StageEnemies:")
for e_id, is_boss in stage_enemies.items():
    print(f" - {e_id:<25} Boss: {is_boss}")
