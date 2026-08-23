import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/SetBonusConfig.json', 'r', encoding='utf-8') as f:
    set_bonuses = json.load(f)

for sname, sdata in set_bonuses.items():
    print(f"=== {sname} ===")
    for pkey in ['Piece2', 'Piece4']:
        if pkey in sdata:
            print(f"  {pkey}: {sdata[pkey].get('StatModifiers')}")
