import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
ws = wb['CombatEvents']

for row in ws.iter_rows(min_row=2):
    if row[0].value == 'psv_triple_edged_blade':
        row[3].value = '50.0, 60.0, 70.0, 80.0, 90.0, 100.0'
        print("Updated psv_triple_edged_blade modify_by_upgrade to:", row[3].value)

wb.save('Tool/data/GameConfig.xlsx')
print("Successfully saved GameConfig.xlsx")
