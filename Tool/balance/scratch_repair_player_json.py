import json

player_path = 'Assets/Data/Player.json'

with open(player_path, 'r', encoding='utf-8') as f:
    data = json.load(f)

# Load ItemConfig to know true rarities
with open('Assets/Data/GameConfig/ItemConfig.json', 'r', encoding='utf-8') as f:
    item_config = json.load(f)

armors = data.get('inventory', {}).get('armors', [])
for a in armors:
    tid = a.get('template_id', '')
    cfg = item_config.get(tid, {})
    true_rarity = cfg.get('rarity', 'Legendary')
    
    # Fix Rare
    if a.get('rare') in [0, '0', None, '']:
        a['rare'] = true_rarity
    
    # Fix Equip
    if a.get('equip') is None:
        a['equip'] = ""

weapons = data.get('inventory', {}).get('weapons', [])
for w in weapons:
    if w.get('equip') is None:
        w['equip'] = ""

with open(player_path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2, ensure_ascii=False)

print(f"Successfully repaired {len(armors)} armors and {len(weapons)} weapons in Player.json!")
