import openpyxl
import json
import sys

sys.stdout.reconfigure(encoding='utf-8')

# 1. Update Localizations.xlsx
loc_path = 'Tool/data/Localizations.xlsx'
wb = openpyxl.load_workbook(loc_path)
ws = wb['Battle']

vn_text = "Càn Quét"
en_text = "Sweep"

found = False
for row in ws.iter_rows(values_only=False):
    if row[0].value == 'STR_SWEEP_ATTACK':
        row[1].value = en_text
        row[2].value = vn_text
        found = True
        break

if not found:
    ws.append(['STR_SWEEP_ATTACK', en_text, vn_text])
    print("Added STR_SWEEP_ATTACK to Localizations.xlsx")

wb.save(loc_path)
print("Saved Localizations.xlsx successfully!")
