import json
import random

player_path = 'Assets/Data/Player.json'
item_config_path = 'Assets/Data/GameConfig/ItemConfig.json'

with open(player_path, 'r', encoding='utf-8') as f:
    player_data = json.load(f)

with open(item_config_path, 'r', encoding='utf-8') as f:
    item_config = json.load(f)

def roll_main_stat(part):
    if part == 'Helmet':
        return 'HP'
    elif part == 'Chestplate':
        return 'DEF'
    elif part == 'Gloves':
        return 'ATK'
    elif part == 'Boots':
        return random.choice(['SPEED', 'ATK', 'HP', 'DEF'])
    elif part == 'Belt':
        return random.choice(['ATK', 'HP', 'DEF', 'PENETRATION'])
    elif part == 'Ring':
        return random.choice(['CRIT_RATE', 'CRIT_DMG', 'ATK', 'HP'])
    return 'ATK'

armors = player_data.get('inventory', {}).get('armors', [])

for idx, a in enumerate(armors):
    tid = a.get('template_id', '')
    cfg = item_config.get(tid, {})
    if not cfg: continue
    
    part = cfg.get('armor_data', {}).get('part', 'Helmet')
    
    # If not equipped, re-roll cleanly to give diverse stats
    # If equipped, ensure fixed pieces have correct fixed stats
    if part in ['Helmet', 'Chestplate', 'Gloves']:
        a['main_stat_type'] = roll_main_stat(part)
    else:
        # For Boots, Belt, Ring: if it's HP or invalid, give it a proper random roll
        a['main_stat_type'] = roll_main_stat(part)

with open(player_path, 'w', encoding='utf-8') as f:
    json.dump(player_data, f, indent=2, ensure_ascii=False)

print(f"Successfully re-rolled MainStats for all {len(armors)} armors in Player.json!")
