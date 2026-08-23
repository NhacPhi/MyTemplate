import json

with open('Assets/Data/GameConfig/SubstatPoolConfig.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

for k, v in data.items():
    print(f"=== {k} ===")
    for p in v['Pools']:
        print(f"  {p['stat_type']:<15} {p['modifier_type']:<10} Weight={p.get('weight')}")
