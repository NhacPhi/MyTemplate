import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
ws = wb['CharacterStat']

# Header: ('ID', 'Rare', 'Type', 'Class', 'Stat_Bias', 'hp', 'atk', 'def', 'speed', 'def_shred', 'crit_rare', 'crit_dmg', 'penetration', 'crit_dmg_res')
# Col indices (1-based):
# 1: ID, 2: Rare, 3: Type, 4: Class, 5: Stat_Bias, 6: hp, 7: atk, 8: def, 9: speed, 10: def_shred, 11: crit_rare, 12: crit_dmg, 13: penetration, 14: crit_dmg_res

for row in ws.iter_rows(min_row=2):
    char_id = row[0].value
    cls = row[3].value
    if cls == 'Boss':
        row[10].value = 50 # crit_rare = 50%
        if row[2].value == 'Tanker':
            row[13].value = 60 # crit_dmg_res = 60% for Tanker Boss
        else:
            row[13].value = 50 # crit_dmg_res = 50% for Fighter/Assassin Boss
        print(f"Updated Boss {char_id}: crit_rate = {row[10].value}%, crit_dmg_res = {row[13].value}%")

wb.save('Tool/data/GameConfig.xlsx')
print("Successfully saved CharacterStat changes to GameConfig.xlsx")
