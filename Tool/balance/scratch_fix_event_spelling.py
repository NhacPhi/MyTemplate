import openpyxl
import sys

sys.stdout.reconfigure(encoding='utf-8')

gc_path = 'Tool/data/GameConfig.xlsx'
wb = openpyxl.load_workbook(gc_path)
ws = wb['CombatEvents']

for row in ws.iter_rows(values_only=False):
    if row[0].value == 'psv_vanguard_of_volition' and row[1].value == 'OnAfterSkillExcute':
        row[1].value = 'OnAfterSkillExecute'
        print("Updated OnAfterSkillExcute -> OnAfterSkillExecute for psv_vanguard_of_volition")
    elif row[1].value == 'OnAfterSkillExcute':
        row[1].value = 'OnAfterSkillExecute'
        print(f"Updated OnAfterSkillExcute -> OnAfterSkillExecute for {row[0].value}")

wb.save(gc_path)
print("Saved GameConfig.xlsx successfully!")
