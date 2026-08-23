import sys
sys.stdout.reconfigure(encoding='utf-8')

chars = [
    {'name': 'Na Tra (Sát Thủ SSR 6sao - ATK 11.5k)', 'hp': 16774, 'atk': 11457, 'def_': 801, 'speed': 125, 'def_shred': 296, 'pen': 51, 'cr': 76, 'cd': 344, 'cr_res': 0, 'star': 6},
    {'name': 'Dương Tiễn (Đấu Sĩ UR 6sao - ATK 11.2k)', 'hp': 31956, 'atk': 11203, 'def_': 1393, 'speed': 96, 'def_shred': 11, 'pen': 38, 'cr': 19, 'cd': 299, 'cr_res': 7, 'star': 6},
    {'name': 'Tôn Ngộ Không (Đấu Sĩ UR 6sao - ATK 12.5k)', 'hp': 28000, 'atk': 12500, 'def_': 1100, 'speed': 115, 'def_shred': 20, 'pen': 45, 'cr': 65, 'cd': 320, 'cr_res': 5, 'star': 6},
    {'name': 'Ngưu Ma Vương (Tanker UR 6sao - ATK 7.5k)', 'hp': 55000, 'atk': 7500, 'def_': 2200, 'speed': 90, 'def_shred': 0, 'pen': 10, 'cr': 15, 'cd': 200, 'cr_res': 25, 'star': 6},
    {'name': 'Lý Tịnh (Tanker SSR 6sao - ATK 6.8k)', 'hp': 48000, 'atk': 6800, 'def_': 1900, 'speed': 92, 'def_shred': 0, 'pen': 10, 'cr': 10, 'cd': 200, 'cr_res': 20, 'star': 6},
    {'name': 'Sa Tăng (Hỗ Trợ SSR 6sao - ATK 8.5k)', 'hp': 35000, 'atk': 8500, 'def_': 1200, 'speed': 105, 'def_shred': 0, 'pen': 15, 'cr': 20, 'cd': 200, 'cr_res': 10, 'star': 6},
]

def test_atk_boost(atk_weight, off_mult_scale, hp_w=1.0, def_w=5.0):
    print(f"=== ATK_WEIGHT={atk_weight}, OFF_SCALE={off_mult_scale}, HP_W={hp_w}, DEF_W={def_w} ===")
    for c in chars:
        crit_factor = (c['cr'] / 100.0) * (c['cd'] / 100.0)
        pen_factor = (c['pen'] / 100.0) + (c['def_shred'] / 500.0)
        off_mult = 1.0 + (crit_factor + pen_factor) * off_mult_scale
        off_rating = (c['atk'] * atk_weight) * off_mult
        
        crit_res_mult = 1.0 + (c['cr_res'] / 100.0)
        def_rating = (c['hp'] * hp_w + c['def_'] * def_w) * crit_res_mult
        
        spd_factor = (c['speed'] / 100.0)
        total = (off_rating + def_rating) * spd_factor + c['star'] * 1000.0
        print(f"{c['name']:<42}: Off={off_rating:7.0f} | Def={def_rating:7.0f} | Total CP={total:7.0f}")
    print()

test_atk_boost(atk_weight=1.5, off_mult_scale=1.2, hp_w=1.0, def_w=5.0)
test_atk_boost(atk_weight=1.8, off_mult_scale=1.3, hp_w=1.0, def_w=5.0)
test_atk_boost(atk_weight=2.0, off_mult_scale=1.3, hp_w=1.0, def_w=5.0)
