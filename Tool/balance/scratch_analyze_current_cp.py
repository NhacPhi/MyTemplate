import json, math, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    player_data = json.load(f)

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    char_configs = json.load(f)

with open('Assets/Data/GameConfig/ItemConfig.json', 'r', encoding='utf-8') as f:
    item_configs = json.load(f)

with open('Assets/Data/GameConfig/SubstatPoolConfig.json', 'r', encoding='utf-8') as f:
    substat_pools = json.load(f)

with open('Assets/Data/GameConfig/SetBonusConfig.json', 'r', encoding='utf-8') as f:
    set_bonuses = json.load(f)

weapons_dict = {w.get('uuid') or w.get('instance_id'): w for w in player_data['inventory']['weapons']}
armors_dict = {a.get('instance_id') or a.get('uuid'): a for a in player_data['inventory']['armors']}

def is_percent_stat(stat_type, mod_type='Flat'):
    if mod_type == 'Percent': return True
    return stat_type in ['CRIT_RATE', 'CRIT_DMG', 'PENETRATION', 'CRIT_DMG_RES', 'EHR', 'RES']

def get_appropriate_armor_main_base_value(stat_type, mod_type, fallback):
    if is_percent_stat(stat_type, mod_type):
        if stat_type in ['CRIT_RATE', 'CRIT_DMG']: return 2.5
        return 3.5
    else:
        mapping = {'SPEED': 3.0, 'ATK': 20.0, 'HP': 100.0, 'DEF': 15.0, 'DEF_SHRED': 30.0}
        return mapping.get(stat_type, fallback if fallback > 0 else 20.0)

def get_armor_growth_per_level(rare):
    return {'Common': 0.1, 'Uncommon': 0.15, 'Rare': 0.2, 'Epic': 0.25, 'Legendary': 0.3}.get(rare, 0.2)

def get_armor_main_stat_by_level(base_val, level, rare):
    growth = get_armor_growth_per_level(rare)
    return round(base_val * (1.0 + (level - 1) * growth))

def calc_char(cid):
    c_save = next((c for c in player_data['roster']['characters'] if c['id'] == cid), None)
    if not c_save: return
    c_cfg = char_configs[cid]
    
    level = c_save['level']
    star = c_save.get('star_up', 0)
    base_stats = c_cfg['stats']
    upgrades = c_cfg.get('upgrades', {})
    
    # 1. Base + Level growth
    stats_flat = {}
    stats_percent = {}
    for st in ['hp', 'atk', 'def', 'speed', 'def_shred', 'crit_rate', 'crit_dmg', 'penetration', 'crit_dmg_res', 'ehr', 'res']:
        b = base_stats.get(st, 0)
        u = upgrades.get(st, 0)
        # stat_growth formula: Level * upgrade
        stats_flat[st.upper()] = b + level * u
        stats_percent[st.upper()] = 0.0

    # 2. Weapon
    w_inst = weapons_dict.get(c_save.get('weapon'))
    if w_inst:
        w_cfg = item_configs.get(w_inst['template_id'], {}).get('weapon_data', {})
        w_lvl = w_inst.get('current_level', 1)
        w_base = w_cfg.get('stats', {})
        w_up = w_cfg.get('upgrades', {})
        for st_name, val in w_base.items():
            up_val = w_up.get(st_name, 0)
            stats_flat[st_name.upper()] = stats_flat.get(st_name.upper(), 0) + val + w_lvl * up_val

    # 3. Armors
    set_counts = {}
    for eq in c_save.get('armors', []):
        a_inst = armors_dict.get(eq['id'])
        if not a_inst: continue
        a_cfg = item_configs.get(a_inst['template_id'], {})
        adata = a_cfg.get('armor_data', {})
        rare = a_inst.get('rare', 'Common')
        lvl = a_inst.get('level', 1)
        
        # Set count
        sname = adata.get('armor_set')
        if sname: set_counts[sname] = set_counts.get(sname, 0) + 1
        
        # Main Stat
        main_type = a_inst.get('main_stat_type') or adata.get('main_stat', {}).get('type', 'ATK').upper()
        mod_type = adata.get('main_stat', {}).get('mod_type', 'Flat')
        raw_val = adata.get('main_stat', {}).get('value', 0)
        base_v = get_appropriate_armor_main_base_value(main_type, mod_type, raw_val)
        calc_v = get_armor_main_stat_by_level(base_v, lvl, rare)
        
        if is_percent_stat(main_type, mod_type):
            stats_percent[main_type] = stats_percent.get(main_type, 0) + calc_v
        else:
            stats_flat[main_type] = stats_flat.get(main_type, 0) + calc_v
            
        # Substats
        pool_id = adata.get('substat_pool_id')
        pool_data = substat_pools.get(pool_id, {}).get('Pools', [])
        for sub in a_inst.get('substats', []):
            st_type = sub['type'].upper()
            st_mod = sub.get('modifier_type', 'Flat')
            st_lvl = max(1, sub.get('level', 1))
            
            p_comp = next((p for p in pool_data if p['stat_type'].upper() == st_type and p['modifier_type'] == st_mod), None) or \
                     next((p for p in pool_data if p['stat_type'].upper() == st_type), None)
            if p_comp:
                avg = (p_comp['min'] + p_comp['max']) * 0.5
                val = round(avg * st_lvl)
            else:
                val = sub.get('value', 0)
                
            if is_percent_stat(st_type, st_mod):
                stats_percent[st_type] = stats_percent.get(st_type, 0) + val
            else:
                stats_flat[st_type] = stats_flat.get(st_type, 0) + val

    # 4. Set bonuses
    for sname, count in set_counts.items():
        s_cfg = set_bonuses.get(sname, {})
        for b_key in ['Piece2', 'Piece4']:
            req_pcs = 2 if b_key == 'Piece2' else 4
            if count >= req_pcs and b_key in s_cfg:
                for b_mod in s_cfg[b_key].get('StatModifiers', []):
                    st = b_mod['Type'].upper()
                    mt = b_mod.get('ModifierType', 'Percent')
                    v = b_mod.get('Value', 0)
                    if is_percent_stat(st, mt):
                        stats_percent[st] = stats_percent.get(st, 0) + v
                    else:
                        stats_flat[st] = stats_flat.get(st, 0) + v

    # Final Total Stats
    total_hp = stats_flat.get('HP', 0) * (1.0 + stats_percent.get('HP', 0) / 100.0)
    total_atk = stats_flat.get('ATK', 0) * (1.0 + stats_percent.get('ATK', 0) / 100.0)
    total_def = stats_flat.get('DEF', 0) * (1.0 + stats_percent.get('DEF', 0) / 100.0)
    total_spd = stats_flat.get('SPEED', 0) * (1.0 + stats_percent.get('SPEED', 0) / 100.0)
    total_cr = max(0, min(100, stats_flat.get('CRIT_RATE', 0) + stats_percent.get('CRIT_RATE', 0)))
    total_cd = stats_flat.get('CRIT_DMG', 0) + stats_percent.get('CRIT_DMG', 0)
    total_pen = stats_flat.get('PENETRATION', 0) + stats_percent.get('PENETRATION', 0)
    total_def_shred = stats_flat.get('DEF_SHRED', 0) + stats_percent.get('DEF_SHRED', 0)
    total_crit_res = stats_flat.get('CRIT_DMG_RES', 0) + stats_percent.get('CRIT_DMG_RES', 0)
    
    # Calculate Power
    crit_factor = (total_cr / 100.0) * (total_cd / 100.0)
    pen_factor = (total_pen / 100.0) + (total_def_shred / 500.0)
    off_mult = 1.0 + (crit_factor + pen_factor) * 1.2
    off_rating = (total_atk * 2.0) * off_mult
    
    def_rating = (total_hp * 2.0 + total_def * 10.0) * (1.0 + total_crit_res / 100.0)
    speed_factor = total_spd / 100.0
    star_bonus = star * 1000.0
    total_power = (off_rating + def_rating) * speed_factor + star_bonus
    
    print(f"\n==================== {cid} (Rare: {c_cfg['rare']}, Role: {c_cfg['type']}) ====================")
    print(f"  HP: {total_hp:.0f}, ATK: {total_atk:.0f}, DEF: {total_def:.0f}, SPD: {total_spd:.0f}")
    print(f"  CR: {total_cr:.1f}%, CD: {total_cd:.1f}%, PEN: {total_pen:.1f}%, SHRED: {total_def_shred:.1f}")
    print(f"  Offensive Rating: {off_rating:.0f} (Mult: {off_mult:.2f})")
    print(f"  Defensive Rating: {def_rating:.0f}")
    print(f"  Speed Factor: {speed_factor:.2f}")
    print(f"  >>> TOTAL COMBAT POWER: {total_power:,.0f} <<<")

for cid in ['SunWukong', 'ErlangShen', 'ThirdPrinceNezha', 'BullDemonKing']:
    calc_char(cid)
