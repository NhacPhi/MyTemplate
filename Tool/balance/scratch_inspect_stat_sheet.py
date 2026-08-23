import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx', data_only=True)
ws = wb['CharacterStat']

for r in ws.iter_rows(values_only=True):
    if r[0]:
        print(r)
