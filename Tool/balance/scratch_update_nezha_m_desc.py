import openpyxl
import sys

sys.stdout.reconfigure(encoding='utf-8')

loc_path = 'Tool/data/Localizations.xlsx'
wb = openpyxl.load_workbook(loc_path)
ws = wb['Battle']

vn_desc = "Phóng Vòng Càn Khôn xoay chuyển cực tốc lao về phía mục tiêu, gây ST bằng {0}% Tấn Công khi bay đi, và gây thêm 1 lần sát thương Chắc Chắn Bạo Kích khi bay ngược trở về."
en_desc = "Hurls the Universe Ring forward dealing {0}% ATK on the outward trip, and deals an additional Guaranteed Critical Hit upon returning."

found = False
for row in ws.iter_rows(values_only=False):
    if row[0].value == 'ThirdPrinceNezha_M_Des':
        row[1].value = en_desc
        row[2].value = vn_desc
        found = True
        break

if not found:
    ws.append(['ThirdPrinceNezha_M_Des', en_desc, vn_desc])

wb.save(loc_path)
print("Updated Localizations.xlsx for ThirdPrinceNezha_M_Des")
