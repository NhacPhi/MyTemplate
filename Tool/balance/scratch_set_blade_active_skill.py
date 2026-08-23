import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
ws = wb['CombatEvents']

for row in ws.iter_rows(min_row=2):
    if row[0].value == 'psv_triple_edged_blade':
        row[4].value = 'ActiveSkill, SingleTarget'
        print("Updated psv_triple_edged_blade condition_filter to:", row[4].value)

wb.save('Tool/data/GameConfig.xlsx')
print("Successfully saved GameConfig.xlsx")
