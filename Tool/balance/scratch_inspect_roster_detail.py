import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    player_data = json.load(f)

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    char_configs = json.load(f)

chars = player_data.get('roster', {}).get('characters', [])
print(f"Total characters in roster: {len(chars)}")

for cdata in chars:
    cid = cdata.get('character_id')
    cfg = char_configs.get(cid, {})
    print(f"=== {cid} ===")
    print(f"  Level: {cdata.get('level')}, Star: {cdata.get('star')}, Rare: {cfg.get('rarity')}, Role: {cfg.get('role')}")
    print(f"  Weapon: {cdata.get('weapon')}")
    print(f"  Armors: {len(cdata.get('armors', []))} pieces")
