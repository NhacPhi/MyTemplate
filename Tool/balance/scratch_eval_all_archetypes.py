import sys
sys.stdout.reconfigure(encoding='utf-8')

chars = [
    {'name': 'Na Tra (Sát Thủ SSR 6sao)', 'hp': 16774, 'atk': 11457, 'def_': 801, 'speed': 125, 'def_shred': 296, 'pen': 51, 'cr': 76, 'cd': 344, 'cr_res': 0, 'star': 6},
    {'name': 'Dương Tiễn (Đấu Sĩ UR 6sao)', 'hp': 31956, 'atk': 11203, 'def_': 1393, 'speed': 96, 'def_shred': 11, 'pen': 38, 'cr': 19, 'cd': 299, 'cr_res': 7, 'star': 6},
    {'name': 'Tôn Ngộ Không (Đấu Sĩ UR 6sao)', 'hp': 28000, 'atk': 12500, 'def_': 1100, 'speed': 115, 'def_shred': 20, 'pen': 45, 'cr': 65, 'cd': 320, 'cr_res': 5, 'star': 6},
    {'name': 'Ngưu Ma Vương (Tanker UR 6sao)', 'hp': 55000, 'atk': 7500, 'def_': 2200, 'speed': 90, 'def_shred': 0, 'pen': 10, 'cr': 15, 'cd': 200, 'cr_res': 25, 'star': 6},
    {'name': 'Lý Tịnh (Tanker SSR 6sao)', 'hp': 48000, 'atk': 6800, 'def_': 1900, 'speed': 92, 'def_shred': 0, 'pen': 10, 'cr': 10, 'cd': 200, 'cr_res': 20, 'star': 6},
    {'name': 'Sa Tăng (Hỗ Trợ SSR 6sao)', 'hp': 35000, 'atk': 8500, 'def_': 1200, 'speed': 105, 'def_shred': 0, 'pen': 15, 'cr': 20, 'cd': 200, 'cr_res': 10, 'star': 6},
]

def eval_formula(hp_weight, def_weight, off_scale=1.0):
    print(f"=== HP_W: {hp_weight}, DEF_W: {def_weight}, OFF_SCALE: {off_scale} ===")
    for c in chars:
        # Base Crit: 150% + cd
        # Average Crit Multiplier: 1.0 + (cr / 100) * ((50 + cd) / 100)
        cm = 1.0 + (c['cr'] / 100.0) * ((50.0 + c['cd']) / 100.0)
        pf = 1.0 + (c['pen'] / 100.0) + (c['def_shred'] / 500.0)
        off_rating = c['atk'] * cm * pf * off_scale
        
        def_rating = (c['hp'] * hp_weight + c['def_'] * def_weight) * (1.0 + c['cr_res'] / 100.0)
        
        spd_factor = c['speed'] / 100.0
        total = (off_rating + def_rating) * spd_factor + c['star'] * 500.0
        print(f"{c['name']:<30}: Off={off_rating:8.0f} | Def={def_rating:8.0f} | Total CP={total:8.0f}")
    print()

eval_formula(hp_weight=0.25, def_weight=2.0, off_scale=1.0)
eval_formula(hp_weight=0.30, def_weight=2.5, off_scale=1.0)
eval_formula(hp_weight=0.35, def_weight=3.0, off_scale=1.0)
eval_formula(hp_weight=0.40, def_weight=3.5, off_scale=1.0)
