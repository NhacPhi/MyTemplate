import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
ws = wb['SkillConfig']

# Update ErlangShen_U, SunWukong_U, ThirdPrinceNezha_B
for row in ws.iter_rows(min_row=2):
    sk_id = row[0].value
    if sk_id == 'ErlangShen_U':
        row[4].value = '1.8, 2.1, 2.4'
        print("Updated ErlangShen_U DmgMult to 1.8, 2.1, 2.4")
    elif sk_id == 'SunWukong_U':
        row[4].value = '2.0, 2.2, 2.5'
        print("Updated SunWukong_U DmgMult to 2.0, 2.2, 2.5")
    elif sk_id == 'ThirdPrinceNezha_B':
        row[4].value = '0.8, 0.9, 1.0'
        print("Updated ThirdPrinceNezha_B DmgMult to 0.8, 0.9, 1.0")

wb.save('Tool/data/GameConfig.xlsx')
print("Successfully saved SkillConfig changes to GameConfig.xlsx")
