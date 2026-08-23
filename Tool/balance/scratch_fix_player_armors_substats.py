import json
import random

player_path = 'Assets/Data/Player.json'
item_config_path = 'Assets/Data/GameConfig/ItemConfig.json'
pool_config_path = 'Assets/Data/GameConfig/SubstatPoolConfig.json'

with open(player_path, 'r', encoding='utf-8') as f:
    player_data = json.load(f)

with open(item_config_path, 'r', encoding='utf-8') as f:
    item_config = json.load(f)

with open(pool_config_path, 'r', encoding='utf-8') as f:
    pool_config = json.load(f)

def get_initial_substat_count(rarity):
    if rarity == 'Legendary': return 4
    if rarity == 'Epic': return 3
    if rarity in ['Rare', 'Uncommon']: return 2
    return 1

def get_random_main_stat(part, default_stat):
    if part == 'Boots':
        return random.choice(['SPEED', 'ATK', 'HP', 'DEF'])
    elif part == 'Ring':
        return random.choice(['CRIT_RATE', 'CRIT_DMG', 'ATK', 'HP'])
    elif part == 'Belt':
        return random.choice(['ATK', 'HP', 'DEF', 'PENETRATION'])
    return default_stat.upper()

armors = player_data.get('inventory', {}).get('armors', [])

for a in armors:
    tid = a.get('template_id', '')
    cfg = item_config.get(tid, {})
    if not cfg: continue
    
    armor_data = cfg.get('armor_data', {})
    part = armor_data.get('part', 'Helmet')
    pool_id = armor_data.get('substat_pool_id', 'Warrior_Pool')
    rarity = cfg.get('rarity', 'Legendary')
    
    a['rare'] = rarity
    if a.get('equip') is None: a['equip'] = ""
    
    # Main stat
    default_main = armor_data.get('main_stat', {}).get('type', 'ATK')
    if not a.get('main_stat_type') or a.get('main_stat_type') == 'None':
        a['main_stat_type'] = get_random_main_stat(part, default_main)
    
    # Substats
    target_count = get_initial_substat_count(rarity)
    pool_entries = pool_config.get(pool_id, {}).get('Pools', [])
    
    if not a.get('substats') or len(a.get('substats')) < target_count:
        existing_types = set()
        substats = []
        if a.get('substats'):
            for s in a['substats']:
                existing_types.add(s['type'])
                substats.append(s)
        
        available = [p for p in pool_entries if p['stat_type'] not in existing_types]
        while len(substats) < target_count and available:
            # Weighted choice
            weights = [p.get('weight', 100) for p in available]
            chosen = random.choices(available, weights=weights, k=1)[0]
            substats.append({
                "type": chosen['stat_type'],
                "modifier_type": chosen['modifier_type'],
                "level": 1
            })
            existing_types.add(chosen['stat_type'])
            available = [p for p in available if p['stat_type'] not in existing_types]
        
        a['substats'] = substats

with open(player_path, 'w', encoding='utf-8') as f:
    json.dump(player_data, f, indent=2, ensure_ascii=False)

print("Successfully regenerated all armor items in Player.json with correct Rarity, Background, Main Stat and 4 Substats!")
