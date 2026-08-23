import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

for cid in ['SunWukong', 'ErlangShen', 'ThirdPrinceNezha', 'BullDemonKing']:
    c = chars.get(cid, {})
    print(f"=== {cid} ===")
    print(f"  Rare: {c.get('rare')}, Role: {c.get('type')}")
    print(f"  BaseStats: {c.get('stats')}")
    print(f"  Upgrades: {c.get('upgrades')}")
