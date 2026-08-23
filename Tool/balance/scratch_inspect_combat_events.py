import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
ws = wb['CombatEvents']

for r in ws.iter_rows(values_only=True):
    if r[0] == 'psv_triple_edged_blade':
        print(r)
