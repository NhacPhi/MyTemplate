import json

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

for idx, a in enumerate(data['inventory']['armors']):
    if '07' in a['template_id'] or '08' in a['template_id']:
        print(f"Armor {idx}: {a['template_id']} -> Rare: {a.get('rare')}, Main: {a.get('main_stat_type')}, Substats: {a.get('substats')}")
