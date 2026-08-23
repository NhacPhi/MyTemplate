import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx', data_only=True)
ws = wb['Character']

print(f"{'ID':<25} {'Name':<30} {'Rare':<8} {'Type':<12} {'Class':<12}")
print("-" * 90)
for r in ws.iter_rows(values_only=True):
    if r[0] and r[0] != 'ID':
        print(f"{str(r[0]):<25} {str(r[1]):<30} {str(r[2]):<8} {str(r[3]):<12} {str(r[4]):<12}")
