import openpyxl, json, sys
sys.stdout.reconfigure(encoding='utf-8')

# 1. Update GameConfig.xlsx SubstatPool with balanced values
wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
ws = wb['SubstatPool']

# Find columns
header = [cell.value for cell in ws[2]]
print("Header:", header)

# We want to balance CRIT_DMG, CRIT_RATE, DEF_SHRED, PENETRATION in SubstatPool
# Warrior_Pool: CRIT_RATE (2-4%), CRIT_DMG (4-8%), PENETRATION (2-5%), DEF_SHRED (5-12%)
# Assassin_Pool: CRIT_RATE (2-5%), CRIT_DMG (5-10%), PENETRATION (3-6%), DEF_SHRED (8-18%)

for row in ws.iter_rows(min_row=3):
    pool_id = row[0].value
    stat_type = row[1].value
    mod_type = row[4].value if len(row) > 4 else None
    
    if pool_id == 'Assassin_Pool':
        if stat_type == 'CRIT_DMG':
            row[2].value = 4.0 # Min
            row[3].value = 8.0 # Max
        elif stat_type == 'CRIT_RATE':
            row[2].value = 2.0
            row[3].value = 4.0
        elif stat_type == 'DEF_SHRED':
            row[2].value = 6.0
            row[3].value = 15.0
        elif stat_type == 'PENETRATION':
            row[2].value = 2.0
            row[3].value = 5.0
            
    elif pool_id == 'Warrior_Pool':
        if stat_type == 'CRIT_DMG':
            row[2].value = 3.0
            row[3].value = 7.0
        elif stat_type == 'CRIT_RATE':
            row[2].value = 1.5
            row[3].value = 3.5
        elif stat_type == 'DEF_SHRED':
            row[2].value = 4.0
            row[3].value = 10.0
        elif stat_type == 'PENETRATION':
            row[2].value = 2.0
            row[3].value = 4.0

wb.save('Tool/data/GameConfig.xlsx')
print("Successfully updated SubstatPool in Tool/data/GameConfig.xlsx")
