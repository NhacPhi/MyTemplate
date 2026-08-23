import json, sys
sys.stdout.reconfigure(encoding='utf-8')

# Player Party (4 characters) average damage output per round:
# Turn 1: Normal skills + 1-2 Ultimates
# Turn 2: Major skills + Cool downs
# Turn 3: 2-3 Ultimates

team_damage_per_round = {
    'FullGear_Endgame': {
        'Round 1 (Opener)': 65000,
        'Round 2 (Sustain)': 45000,
        'Round 3 (Burst Ults)': 85000,
        'Round 4 (Sustain)': 45000
    },
    'MidGear_Progression': {
        'Round 1 (Opener)': 25000,
        'Round 2 (Sustain)': 18000,
        'Round 3 (Burst Ults)': 35000,
        'Round 4 (Sustain)': 18000
    }
}

options = [
    {'name': 'Option 1: Nhẹ nhàng (5.0x HP)', 'hp_mult': 5.0, 'def_mult': 1.6, 'res': 20},
    {'name': 'Option 2: Vừa phải / Chuẩn turn-based (7.5x HP)', 'hp_mult': 7.5, 'def_mult': 2.0, 'res': 25},
    {'name': 'Option 3: Thử thách (10.0x HP)', 'hp_mult': 10.0, 'def_mult': 2.2, 'res': 30},
    {'name': 'Option 4: Đậm chất Boss Raid (14.0x HP)', 'hp_mult': 14.0, 'def_mult': 2.6, 'res': 40},
]

base_hp_raw = 2750 * 1.45 # SSR Fighter base = ~3,988
level = 20

print("=== SO SÁNH CÁC MỨC HỆ SỐ MÁU BOSS (Ở LEVEL 20) ===\n")

for opt in options:
    base_hp = round(base_hp_raw * opt['hp_mult'])
    upg_hp = round(base_hp * 0.05)
    total_hp = base_hp + level * upg_hp
    
    # Calculate rounds to kill for Full Gear team:
    dmg_accum = 0
    rounds_needed = 0
    rounds_data = team_damage_per_round['FullGear_Endgame']
    for r_idx, (r_name, r_dmg) in enumerate(rounds_data.items(), 1):
        dmg_accum += r_dmg
        if dmg_accum >= total_hp:
            rounds_needed = r_idx - 1 + (total_hp - (dmg_accum - r_dmg)) / r_dmg
            break
    if rounds_needed == 0:
        rounds_needed = 4 + (total_hp - dmg_accum) / 45000
        
    print(f"🔹 {opt['name']}:")
    print(f"   - Máu Boss (Lv 20): {total_hp:,} HP")
    print(f"   - Số hiệp để cả đội 4 tướng Full Đồ hạ gục: ~{rounds_needed:.1f} Hiệp (Rounds)")
    if rounds_needed < 1.0:
        print(f"   - Đánh giá: ⚠️ Quá nhanh, Boss chết ngay giữa Hiệp 1 trước khi kịp ra chiêu!")
    elif rounds_needed <= 2.0:
        print(f"   - Đánh giá: ✅ Cực kỳ vừa vặn cho ải cốt truyện thông thường (Boss sống qua hiệp 1, tung được 1 chiêu nộ).")
    elif rounds_needed <= 3.5:
        print(f"   - Đánh giá: 🔥 Tuyệt vời cho Boss Ải khó / Thử thách (Giao tranh kéo dài 2-3 hiệp, cần tanker & hồi máu).")
    else:
        print(f"   - Đánh giá: ⚔️ Rất trâu, phù hợp cho Boss Thế Giới / Boss Leo Tháp.")
    print()
