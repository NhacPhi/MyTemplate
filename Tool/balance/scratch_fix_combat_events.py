import openpyxl
import sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
ws_ce = wb['CombatEvents']

# Find and fix psv_bulldemonking_counter row
for row in ws_ce.iter_rows(min_row=2):
    if row[0].value == 'psv_bulldemonking_counter':
        row[1].value = 'OnTakeDamage'
        row[2].value = 'Eff_CounterAttack'
        row[3].value = '50, 75, 100, 100, 100, 100'
        row[4].value = 'BasicAttack'
        row[5].value = 0
        row[6].value = 0
        row[7].value = 'enemy'
        row[8].value = 'Phản đòn khi bị đánh thường'
        print('Fixed psv_bulldemonking_counter in CombatEvents')

wb.save('Tool/data/GameConfig.xlsx')
print('Saved GameConfig.xlsx successfully!')
