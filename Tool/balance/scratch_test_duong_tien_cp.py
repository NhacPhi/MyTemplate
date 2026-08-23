import sys
sys.stdout.reconfigure(encoding='utf-8')

# Duong Tien & Na Tra test
dt = {'name': 'Dương Tiễn (UR)', 'hp': 31956, 'atk': 11203, 'def_': 1393, 'speed': 96, 'def_shred': 11, 'pen': 38, 'cr': 19, 'cd': 299, 'cr_res': 7, 'star': 6}
nt = {'name': 'Na Tra (SSR)', 'hp': 16774, 'atk': 11457, 'def_': 801, 'speed': 125, 'def_shred': 296, 'pen': 51, 'cr': 76, 'cd': 344, 'cr_res': 0, 'star': 6}
wk = {'name': 'Ngộ Không (UR)', 'hp': 28000, 'atk': 12500, 'def_': 1100, 'speed': 115, 'def_shred': 20, 'pen': 45, 'cr': 65, 'cd': 320, 'cr_res': 5, 'star': 6}
cow = {'name': 'Ngưu Ma Vương (UR)', 'hp': 55000, 'atk': 7500, 'def_': 2200, 'speed': 90, 'def_shred': 0, 'pen': 10, 'cr': 15, 'cd': 200, 'cr_res': 25, 'star': 6}

def sim(hp_w, def_w, atk_w, off_scale, speed_base=0.5):
    # speed_factor = (1.0 + (speed - 100) * 0.005) or similar so 96 speed isn't punishing
    print(f"=== hp_w={hp_w}, def_w={def_w}, atk_w={atk_w}, off_scale={off_scale} ===")
    for c in [dt, nt, wk, cow]:
        crit_factor = (c['cr'] / 100.0) * (c['cd'] / 100.0)
        pen_factor = (c['pen'] / 100.0) + (c['def_shred'] / 500.0)
        off_mult = 1.0 + (crit_factor + pen_factor) * off_scale
        off_rating = (c['atk'] * atk_w) * off_mult
        
        crit_res_mult = 1.0 + (c['cr_res'] / 100.0)
        def_rating = (c['hp'] * hp_w + c['def_'] * def_w) * crit_res_mult
        
        # Smooth speed factor: baseline 1.0 at 100 speed, softly scales
        spd_factor = 0.8 + (c['speed'] / 500.0) # 96 spd -> 0.992, 125 spd -> 1.05
        # or spd_factor = max(1.0, c['speed'] / 100.0)
        
        total = (off_rating + def_rating) * (c['speed'] / 100.0) + c['star'] * 1000.0
        print(f"{c['name']:<20}: Off={off_rating:7.0f} | Def={def_rating:7.0f} | Total CP={total:7.0f}")
    print()

# Let's test increasing HP and DEF weight so 32k HP and 1.4k DEF gives significant power
sim(hp_w=2.0, def_w=10.0, atk_w=2.0, off_scale=1.2)
sim(hp_w=2.5, def_w=12.0, atk_w=2.2, off_scale=1.2)
sim(hp_w=3.0, def_w=15.0, atk_w=2.5, off_scale=1.2)
