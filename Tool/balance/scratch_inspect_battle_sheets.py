import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx', data_only=True)

for sheet in ['BattleConfig', 'StageEnemies', 'Character', 'CharacterStat', 'CharacterUpgrade']:
    ws = wb[sheet]
    print(f"\n==================== SHEET: {sheet} ====================")
    rows = list(ws.iter_rows(values_only=True))
    for r in rows[:15]:
        print(r)
