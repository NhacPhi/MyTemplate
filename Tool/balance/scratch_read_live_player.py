import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

armors = data.get('inventory', {}).get('armors', [])
print(f"Total armors in Player.json: {len(armors)}")

for idx, a in enumerate(armors):
    print(f"[{idx}] {a.get('template_id'):<20} Level={a.get('level')} MainStat={a.get('main_stat_type')} Rare={a.get('rare')}")
