import sys
sys.stdout.reconfigure(encoding='utf-8')

def test_formulas(hp, atk, def_, speed, def_shred, pen, crit_rate, crit_dmg, crit_res, star_level=6):
    # CÔNG THỨC HIỆN TẠI
    crit_mult = 1.0 + (crit_rate / 100.0) * ((50.0 + crit_dmg) / 100.0)
    pen_factor = 1.0 + (pen / 100.0) + (def_shred / 100.0)
    off_power_cur = atk * crit_mult * pen_factor
    ehp_cur = hp * (1.0 + (def_ / 400.0))
    def_power_cur = (ehp_cur / 2.5) * (1.0 + (crit_res / 100.0))
    cur_total = (off_power_cur + def_power_cur) * (speed / 100.0) + star_level * 250.0

    # CÔNG THỨC MỚI CÂN BẰNG:
    # 1. DEF_SHRED là Flat nên quy đổi theo chuẩn 500 DEF: def_shred / 500
    pen_factor_new = 1.0 + (pen / 100.0) + (def_shred / 500.0)
    # 2. Crit Multiplier:
    crit_mult_new = 1.0 + (crit_rate / 100.0) * (crit_dmg / 100.0)
    off_power_new = atk * crit_mult_new * pen_factor_new
    
    # 3. EHP tính đúng trọng số sinh tồn (HP * (1 + DEF / 500))
    ehp_new = hp * (1.0 + (def_ / 500.0))
    def_power_new = ehp_new * (1.0 + (crit_res / 100.0))
    
    # 4. Tổng chiến lực: Sát Thương (50%) + Sinh Tồn (50%) nhân với Tốc Độ
    speed_factor_new = (speed / 100.0)
    new_total = (off_power_new * 0.6 + def_power_new * 0.4) * speed_factor_new + star_level * 500.0

    print(f"Offensive: {off_power_new:.0f}, Defensive: {def_power_new:.0f}")
    return cur_total, new_total

print("=== NA TRA ===")
cur_nt, new_nt = test_formulas(16774, 11457, 801, 125, 296, 51, 76, 344, 0)
print(f"Hien tai: {cur_nt:.0f} | De xuat can bang: {new_nt:.0f}\n")

print("=== DUONG TIEN ===")
cur_dt, new_dt = test_formulas(31956, 11203, 1393, 96, 11, 38, 19, 299, 7)
print(f"Hien tai: {cur_dt:.0f} | De xuat can bang: {new_dt:.0f}\n")
