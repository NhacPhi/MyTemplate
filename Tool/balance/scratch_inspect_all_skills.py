import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx', data_only=True)
ws = wb['SkillConfig']

print(f"{'ID':<25} {'Type':<15} {'Target':<15} {'Mult':<20} {'CD':<10}")
print("-" * 85)
for r in ws.iter_rows(values_only=True):
    if r[0] and r[0] != 'ID':
        print(f"{str(r[0]):<25} {str(r[3]):<15} {str(r[5]):<15} {str(r[6]):<20} {str(r[7]):<10}")
