import json

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

armors = data.get('inventory', {}).get('armors', [])
for a in armors:
    tid = a.get('template_id', '')
    if any(s in tid for s in ['01', '07', '08']):
        print(f"{tid:<22}: MainStatType={a.get('main_stat_type')}")
