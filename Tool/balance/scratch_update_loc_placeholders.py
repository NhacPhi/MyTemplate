import openpyxl
import sys

sys.stdout.reconfigure(encoding='utf-8')

# Update Localizations.xlsx
loc_path = 'Tool/data/Localizations.xlsx'
wb_loc = openpyxl.load_workbook(loc_path)
ws_str = wb_loc['STR']

for row in ws_str.iter_rows(values_only=False):
    if row[0].value == 'STR_WINDFIRE_WHEEL_SKILL':
        row[1].value = 'Increases CRIT DMG by {0}% and ATK by {1}%. Landing a Critical Hit increases self Action Point by {2}%.'
        row[2].value = 'Tăng {0}% Sát Thương Bạo Kích và {1}% Tấn Công. Khi gây sát thương Bạo Kích, tăng {2}% Điểm Hành Động của bản thân.'
        print("Updated STR_WINDFIRE_WHEEL_SKILL")
    elif row[0].value == 'STR_NINE_TOOLTHED_RAKE_SKILL':
        row[1].value = 'Increases Max HP by {0}% and DEF by {1}%. Each time taking damage, increases DEF by {2}%, stacking up to 5 times. At 5 stacks, restores 10% Max HP.'
        row[2].value = 'Tăng {0}% HP và {1}% Phòng Thủ. Mỗi khi chịu sát thương, Phòng Thủ tăng thêm {2}%, tối đa cộng dồn 5 tầng. Khi đạt 5 tầng, hồi phục 10% HP Tối Đa.'
        print("Updated STR_NINE_TOOLTHED_RAKE_SKILL")

wb_loc.save(loc_path)
print("Saved Localizations.xlsx successfully!")
