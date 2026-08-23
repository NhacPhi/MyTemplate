import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

# Update SubstatPool in GameConfig.xlsx for PENETRATION
wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
ws = wb['SubstatPool']

for row in ws.iter_rows(min_row=3):
    pool_id = row[0].value
    stat_type = row[1].value
    
    if stat_type == 'PENETRATION':
        if pool_id == 'Assassin_Pool':
            row[2].value = 1.0 # Min
            row[3].value = 2.5 # Max
            print("Set Assassin_Pool PENETRATION to 1.0 - 2.5%")
        elif pool_id == 'Warrior_Pool':
            row[2].value = 0.8 # Min
            row[3].value = 2.0 # Max
            print("Set Warrior_Pool PENETRATION to 0.8 - 2.0%")

wb.save('Tool/data/GameConfig.xlsx')
print("Successfully saved PENETRATION changes in GameConfig.xlsx")
