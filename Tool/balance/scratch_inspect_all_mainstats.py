import json

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

for idx, a in enumerate(data['inventory']['armors']):
    print(f"Armor {idx}: {a.get('template_id')}, MainStat={a.get('main_stat_type')}")
