import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx', data_only=True)
ws = wb['StageEnemies']

print(f"{'BattleID':<30} {'Slot':<6} {'Enemy_ID':<25} {'Level':<6} {'Boss':<6}")
print("-" * 75)
for r in ws.iter_rows(values_only=True):
    if r[0] and r[0] != 'BattleID':
        print(f"{str(r[0]):<30} {str(r[1]):<6} {str(r[2]):<25} {str(r[3]):<6} {str(r[4]):<6}")
