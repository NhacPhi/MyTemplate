import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

for cid in ['SunWukong', 'ErlangShen', 'ThirdPrinceNezha', 'BullDemonKing']:
    c = chars.get(cid, {})
    print(f"=== {cid} ===")
    print(f"  Rarity: {c.get('rarity')}, Role: {c.get('role')}")
    print(f"  BaseStats: {c.get('base_stats')}")
    print(f"  Growth: {c.get('stat_growth')}")
