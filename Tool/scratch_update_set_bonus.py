import openpyxl
import sys

sys.stdout.reconfigure(encoding='utf-8')

gc_path = 'Tool/data/GameConfig.xlsx'
wb = openpyxl.load_workbook(gc_path)
ws = wb['SetBonusConfig']

for row in ws.iter_rows(values_only=False):
    set_id = row[0].value
    if set_id == 'Armor01':
        # Kim Vũ Long Lân Khải -> Tăng 20% ATK
        row[3].value = 'atk'
        row[4].value = 20
        row[5].value = 'Percent'
        print("Updated Armor01 (Kim Vũ Long Lân Khải) to 20% ATK")
    elif set_id == 'Armor04':
        # Viêm Ngưu Thần Khải -> Tăng 10% HP và 10% DEF
        row[3].value = 'hp, def'
        row[4].value = '10, 10'
        row[5].value = 'Percent, Percent'
        print("Updated Armor04 (Viêm Ngưu Thần Khải) to 10% HP and 10% DEF")

wb.save(gc_path)
print("Saved GameConfig.xlsx successfully.")
