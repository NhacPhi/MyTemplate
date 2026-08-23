import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/SubstatPoolConfig.json', 'r', encoding='utf-8') as f:
    pools = json.load(f)

for pid, pdata in pools.items():
    print(f"=== {pid} ===")
    for p in pdata.get('Pools', []):
        print(f"  {p['stat_type']:<15} {p['modifier_type']:<10} Min={p['min']:<6} Max={p['max']:<6}")
