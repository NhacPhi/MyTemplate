import json, sys
sys.stdout.reconfigure(encoding='utf-8')

# Current test parameters
class_multipliers = {
    'Character': {'hp': 1.0, 'atk': 1.0, 'def': 1.0, 'crit_res': 0},
    'Creep':     {'hp': 3.5, 'atk': 1.3, 'def': 1.8, 'crit_res': 15},
    'Boss':      {'hp': 14.0, 'atk': 1.8, 'def': 2.6, 'crit_res': 40}
}

rarity_mult = {'R': 1.0, 'SR': 1.2, 'SSR': 1.45, 'UR': 1.7}

def sim_enemy(name, role, rare, cls, level):
    base_hp_raw = {'Tanker': 4000, 'Fighter': 2750, 'Assassin': 2500, 'Mage': 2250, 'Support': 3000}[role]
    base_atk_raw = {'Tanker': 900, 'Fighter': 1650, 'Assassin': 2250, 'Mage': 2100, 'Support': 1050}[role]
    base_def_raw = {'Tanker': 375, 'Fighter': 275, 'Assassin': 250, 'Mage': 210, 'Support': 250}[role]
    
    r_m = rarity_mult[rare]
    c_m = class_multipliers[cls]
    
    base_hp = round(base_hp_raw * r_m * c_m['hp'])
    base_atk = round(base_atk_raw * r_m * c_m['atk'])
    base_def = round(base_def_raw * r_m * c_m['def'])
    
    upg_hp = round(base_hp * 0.05)
    upg_atk = round(base_atk * 0.05)
    upg_def = round(base_def * 0.05)
    
    total_hp = base_hp + level * upg_hp
    total_atk = base_atk + level * upg_atk
    total_def = base_def + level * upg_def
    
    print(f"[{cls}] {name} ({rare} {role}, Lv {level}):")
    print(f"   HP: {total_hp:,} (Base: {base_hp:,}, Growth: +{upg_hp}/lv)")
    print(f"   ATK: {total_atk:,} (Base: {base_atk:,}, Growth: +{upg_atk}/lv)")
    print(f"   DEF: {total_def:,} (Base: {base_def:,}, Growth: +{upg_def}/lv)")
    
    # Combat simulation:
    # Player ATK = 9,000, Skill Multiplier = 2.0 (Ultimate)
    player_atk = 9000
    player_pen = 40.0 # 40%
    player_def_shred = 150 # 150 flat
    
    def_after_pen = total_def * (1.0 - player_pen/100.0)
    effective_def = max(0, def_after_pen - player_def_shred)
    def_reduction = 1000.0 / (1000.0 + effective_def)
    
    # Normal hit
    dmg_normal = round(player_atk * 2.0 * def_reduction)
    # Crit hit (130% crit dmg - 40% crit res = 90% bonus -> 2.4x crit mult)
    crit_mult = max(1.0, 1.5 + 1.30 - c_m['crit_res']/100.0)
    dmg_crit = round(player_atk * 2.0 * crit_mult * def_reduction)
    
    turns_to_kill_normal = total_hp / max(1, dmg_normal)
    turns_to_kill_crit = total_hp / max(1, dmg_crit)
    
    print(f"   >> Player Ultimate Damage: Normal={dmg_normal:,} | Crit={dmg_crit:,} (EffDEF: {effective_def:.0f})")
    print(f"   >> Hits required to defeat: ~{turns_to_kill_normal:.1f} Normal Ults | ~{turns_to_kill_crit:.1f} Crit Ults\n")

print("==================== LÍNH (CREEPS) ====================")
sim_enemy("CrabSolider", "Fighter", "R", "Creep", 15)
sim_enemy("YoungBufflalo", "Fighter", "R", "Creep", 20)
sim_enemy("HeavenlySoddier", "Fighter", "R", "Creep", 25)

print("==================== BOSSES ====================")
sim_enemy("GoldfishDemon", "Fighter", "SSR", "Boss", 15)
sim_enemy("ThirdDragonPrince", "Fighter", "UR", "Boss", 20)
sim_enemy("BullDemonKing_Boss", "Tanker", "SSR", "Boss", 30)
sim_enemy("ErlangShen_Boss", "Fighter", "UR", "Boss", 30)
