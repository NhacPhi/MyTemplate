import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx', data_only=True)
ws = wb['SubstatPool']

print(f"{'PoolID':<18} {'Type':<16} {'ModType':<12} {'Min':<8} {'Max':<8} {'Weight':<8}")
print("-" * 75)
for r in ws.iter_rows(values_only=True):
    if r[0] and r[0] != 'PoolID':
        print(f"{str(r[0]):<18} {str(r[1]):<16} {str(r[2]):<12} {str(r[3]):<8} {str(r[4]):<8} {str(r[5]):<8}")
