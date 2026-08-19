import openpyxl
import sys

sys.stdout.reconfigure(encoding='utf-8')

gc_path = 'Tool/data/GameConfig.xlsx'
wb = openpyxl.load_workbook(gc_path)
ws = wb['SkillConfig']

for row in ws.iter_rows(values_only=False):
    if row[0].value == 'ThirdPrinceNezha_M':
        row[4].value = '0.4, 0.5, 0.6'
        print("Updated ThirdPrinceNezha_M multiplier to '0.4, 0.5, 0.6'")
    elif row[0].value == 'ThirdPrinceNezha_U':
        row[4].value = '1.4, 1.6, 1.8'
        print("Updated ThirdPrinceNezha_U multiplier to '1.4, 1.6, 1.8'")

wb.save(gc_path)
print("Saved GameConfig.xlsx successfully!")
