import json, sys
sys.stdout.reconfigure(encoding='utf-8')

# Check all Bosses in Character sheet
import openpyxl

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx', data_only=True)
ws = wb['Character']

bosses = []
for r in ws.iter_rows(values_only=True):
    if r[0] and r[0] != 'ID' and r[4] == 'Boss':
        bosses.append((r[0], r[2], r[3]))

print("List of Bosses:")
for b_id, rare, role in bosses:
    print(f" - {b_id:<25} Rare: {rare:<5} Role: {role}")
