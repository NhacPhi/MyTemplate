import openpyxl
import sys
import time
sys.stdout.reconfigure(encoding='utf-8')

for attempt in range(5):
    try:
        wb_loc = openpyxl.load_workbook('Tool/data/Localizations.xlsx')
        ws_battle = wb_loc['Battle']
        for row in ws_battle.iter_rows(min_row=2):
            if row[0].value == 'BullDemonKing_B_Des':
                row[1].value = 'Swings a massive blade dealing {0}% ATK damage to a single enemy. \\nWhen hit by a basic attack, has a {1}% chance to [Counter-Attack], dealing {2}% ATK damage.'
                row[2].value = 'Vung đại đao chém tựa dời núi lấp biển, gây ST bằng {0}% Tấn Công cho một kẻ địch. \\nKhi bị tấn công thường có {1}% cơ hội [Phản Kích], đòn đánh gây ST {2}% Tấn Công.'
                print('Fixed bracket in BullDemonKing_B_Des in Localizations.xlsx')
        wb_loc.save('Tool/data/Localizations.xlsx')
        print('Successfully saved Localizations.xlsx')
        break
    except PermissionError:
        print(f'Attempt {attempt+1}: Localizations.xlsx locked, retrying in 1s...')
        time.sleep(1)
