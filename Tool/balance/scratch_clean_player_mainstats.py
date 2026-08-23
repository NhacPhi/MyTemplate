import json, random, sys
sys.stdout.reconfigure(encoding='utf-8')

player_path = 'Assets/Data/Player.json'
item_config_path = 'Assets/Data/GameConfig/ItemConfig.json'

with open(player_path, 'r', encoding='utf-8') as f:
    player_data = json.load(f)

with open(item_config_path, 'r', encoding='utf-8') as f:
    item_config = json.load(f)

def get_main_stat_for_part(part):
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
    
    if part in ['Helmet', 'Chestplate', 'Gloves']:
        a['main_stat_type'] = get_main_stat_for_part(part)
    else:
        # Re-roll if it was HP (due to the old enum bug) or None
        if not a.get('main_stat_type') or a.get('main_stat_type') in ['None', 'HP']:
            a['main_stat_type'] = get_main_stat_for_part(part)

with open(player_path, 'w', encoding='utf-8') as f:
    json.dump(player_data, f, indent=2, ensure_ascii=False)

print(f"Successfully cleaned all {len(armors)} armors in Player.json!")
