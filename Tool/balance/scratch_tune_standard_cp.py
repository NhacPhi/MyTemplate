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

def test_formula(atk_weight, hp_weight, def_weight, crit_scale, pen_scale, shred_scale, spd_base, spd_scale, ur_mult, ssr_mult):
    results = []
    for cid in ['SunWukong', 'ErlangShen', 'ThirdPrinceNezha', 'BullDemonKing', 'ZhuBaJie', 'TangSanZang']:
        c_save = next((c for c in player_data['roster']['characters'] if c['id'] == cid), None)
        if not c_save: continue
        c_cfg = char_configs[cid]
        level = c_save['level']
        rare = c_cfg.get('rare', 'SSR')
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
        total_spd = stats_flat.get('SPEED', 0) * (1.0 + stats_percent.get('SPEED', 0) / 100.0)
        total_cr = max(0, min(100, stats_flat.get('CRIT_RATE', 0) + stats_percent.get('CRIT_RATE', 0)))
        total_cd = stats_flat.get('CRIT_DMG', 0) + stats_percent.get('CRIT_DMG', 0)
        total_pen = stats_flat.get('PENETRATION', 0) + stats_percent.get('PENETRATION', 0)
        total_def_shred = stats_flat.get('DEF_SHRED', 0) + stats_percent.get('DEF_SHRED', 0)
        total_crit_res = stats_flat.get('CRIT_DMG_RES', 0) + stats_percent.get('CRIT_DMG_RES', 0)
        total_ehr = stats_flat.get('EHR', 0) + stats_percent.get('EHR', 0)
        total_res = stats_flat.get('RES', 0) + stats_percent.get('RES', 0)
        
        # Offensive
        crit_power = total_atk * (total_cr / 100.0) * (total_cd / 100.0) * crit_scale
        pen_power = total_atk * (total_pen / 100.0) * pen_scale + (total_def_shred * shred_scale)
        offensive_rating = (total_atk * atk_weight) + crit_power + pen_power
        
        # Defensive
        crit_res_power = total_hp * (total_crit_res / 100.0) * 0.3
        defensive_rating = (total_hp * hp_weight) + (total_def * def_weight) + crit_res_power
        
        # Speed
        speed_factor = spd_base + (total_spd / 100.0) * spd_scale
        
        # Rarity
        r_mult = {'UR': ur_mult, 'SSR': ssr_mult, 'SR': 0.85, 'R': 0.70}.get(rare, 1.0)
        
        utility = (total_ehr + total_res) * 20.0
        star_bonus = c_save.get('star_up', 0) * 2000.0
        
        cp = round(((offensive_rating + defensive_rating) * speed_factor + utility) * r_mult + star_bonus)
        results.append((cid, rare, c_cfg['type'], cp, offensive_rating, defensive_rating, total_atk, total_hp))
    return results

print("=== THỬ NGHIỆM CÁC TỶ LỆ TRỌNG SỐ CHIẾN LỰC CHUẨN RPG ===")

# Test Balanced Standard Formula:
# ATK = 4.5, HP = 1.0, DEF = 7.0, CritScale = 1.0, PenScale = 0.6, ShredScale = 25, SpdBase = 0.6, SpdScale = 0.4, UR = 1.25, SSR = 1.0
res = test_formula(atk_weight=4.5, hp_weight=1.0, def_weight=7.0, crit_scale=1.0, pen_scale=0.6, shred_scale=25, spd_base=0.6, spd_scale=0.4, ur_mult=1.25, ssr_mult=1.0)

for cid, rare, role, cp, off, defe, atk, hp in res:
    print(f"  {cid:<20} ({rare:<3} {role:<9}): CP = {cp:>7,} | OffRating = {off:>6,.0f} | DefRating = {defe:>6,.0f} | ATK = {atk:,.0f} | HP = {hp:,.0f}")
