import json

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

inv = data.get('inventory', {})
armors = inv.get('armors', [])
print(f'Total Armors in Player.json: {len(armors)}')
for idx, a in enumerate(armors):
    print(f"Armor {idx}: TemplateID={a.get('template_id')}, Rare={a.get('rare')}, Equip={a.get('equip')}, MainStat={a.get('main_stat_type')}")
