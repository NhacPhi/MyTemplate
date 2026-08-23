import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    player_data = json.load(f)

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    char_configs = json.load(f)

with open('Assets/Data/GameConfig/ItemConfig.json', 'r', encoding='utf-8') as f:
    item_configs = json.load(f)

with open('Assets/Data/GameConfig/SubstatPoolConfig.json', 'r', encoding='utf-8') as f:
    substat_pools = json.load(f)

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

def calc_stats(cid):
    c_save = next((c for c in player_data['roster']['characters'] if c['id'] == cid), None)
    c_cfg = char_configs[cid]
    level = c_save['level']
    base_stats = c_cfg['stats']
    upgrades = c_cfg.get('upgrades', {})
    
    stats_flat = {}
    stats_percent = {}
    for st in ['hp', 'atk', 'def', 'speed', 'def_shred', 'crit_rate', 'crit_dmg', 'penetration', 'crit_dmg_res', 'ehr', 'res']:
        b = base_stats.get(st, 0)
        u = upgrades.get(st, 0)
        stats_flat[st.upper()] = b + level * u
        stats_percent[st.upper()] = 0.0

    w_inst = weapons_dict.get(c_save.get('weapon'))
    if w_inst:
        w_cfg = item_configs.get(w_inst['template_id'], {}).get('weapon_data', {})
        w_lvl = w_inst.get('current_level', 1)
        w_base = w_cfg.get('stats', {})
        w_up = w_cfg.get('upgrades', {})
        for st_name, val in w_base.items():
            up_val = w_up.get(st_name, 0)
            stats_flat[st_name.upper()] = stats_flat.get(st_name.upper(), 0) + val + w_lvl * up_val

    for eq in c_save.get('armors', []):
        a_inst = armors_dict.get(eq['id'])
        if not a_inst: continue
        a_cfg = item_configs.get(a_inst['template_id'], {})
        adata = a_cfg.get('armor_data', {})
        a_rare = a_inst.get('rare', 'Common')
        a_lvl = a_inst.get('level', 1)
        
        main_type = a_inst.get('main_stat_type') or adata.get('main_stat', {}).get('type', 'ATK').upper()
        mod_type = adata.get('main_stat', {}).get('mod_type', 'Flat')
        raw_val = adata.get('main_stat', {}).get('value', 0)
        base_v = get_appropriate_armor_main_base_value(main_type, mod_type, raw_val)
        calc_v = get_armor_main_stat_by_level(base_v, a_lvl, a_rare)
        
        if is_percent_stat(main_type, mod_type):
            stats_percent[main_type] = stats_percent.get(main_type, 0) + calc_v
        else:
            stats_flat[main_type] = stats_flat.get(main_type, 0) + calc_v
            
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

    total_hp = stats_flat.get('HP', 0) * (1.0 + stats_percent.get('HP', 0) / 100.0)
    total_atk = stats_flat.get('ATK', 0) * (1.0 + stats_percent.get('ATK', 0) / 100.0)
    total_def = stats_flat.get('DEF', 0) * (1.0 + stats_percent.get('DEF', 0) / 100.0)
    total_cr = max(0, min(100, stats_flat.get('CRIT_RATE', 0) + stats_percent.get('CRIT_RATE', 0)))
    total_cd = stats_flat.get('CRIT_DMG', 0) + stats_percent.get('CRIT_DMG', 0)
    total_pen = stats_flat.get('PENETRATION', 0) + stats_percent.get('PENETRATION', 0)
    total_def_shred = stats_flat.get('DEF_SHRED', 0) + stats_percent.get('DEF_SHRED', 0)
    
    target_def = 1000.0
    def_after_pen = target_def * (1.0 - total_pen / 100.0)
    eff_def = max(0, def_after_pen - total_def_shred)
    def_factor = 1000.0 / (1000.0 + eff_def)
    
    eff_bonus_cd = min(80.0, total_cd)
    crit_mult = (150.0 + eff_bonus_cd) / 100.0
    
    # Skills:
    sk = c_cfg['skills']
    b_mult = sk['Base']['damage_multiplier'][-1]
    m_mult = sk['Major']['damage_multiplier'][-1]
    u_mult = sk['Ultimate']['damage_multiplier'][-1]
    
    dmg_basic = total_atk * b_mult * def_factor
    dmg_major = total_atk * m_mult * def_factor
    dmg_ult = total_atk * u_mult * def_factor
    dmg_ult_crit = total_atk * u_mult * crit_mult * def_factor
    
    print(f"=== {cid} ({c_cfg['rare']} {c_cfg['type']}) ===")
    print(f"  HP: {total_hp:,.0f} | DEF: {total_def:,.0f} | ATK: {total_atk:,.0f}")
    print(f"  Basic ({b_mult}x): {dmg_basic:,.0f}")
    print(f"  Major ({m_mult}x): {dmg_major:,.0f}")
    print(f"  Ultimate ({u_mult}x): Normal={dmg_ult:,.0f} | Crit={dmg_ult_crit:,.0f}")
    print()

for cid in ['SunWukong', 'ErlangShen', 'ThirdPrinceNezha']:
    calc_stats(cid)
