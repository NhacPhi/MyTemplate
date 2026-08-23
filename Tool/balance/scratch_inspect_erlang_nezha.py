import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx', data_only=True)
ws = wb['SkillConfig']

for r in list(ws.iter_rows(values_only=True))[1:]:
    if r[0] and ('Erlang' in r[0] or 'Nezha' in r[0] or 'SunWukong' in r[0]):
        print(r[0], "=> Type:", r[3], "| DmgMult:", r[4], "| CD:", r[5], "| Target:", r[7])
