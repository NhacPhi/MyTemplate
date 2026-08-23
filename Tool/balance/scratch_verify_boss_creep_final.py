import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

print("=== CHECK CHỈ SỐ BOSS (CRIT RES 25-30%, CRIT RATE 50%) ===")
boss_ids = ['GoldfishDemon', 'ThirdDragonPrince', 'BullDemonKing_Boss', 'ErlangShen_Boss', 'YellowWindMonster', 'MarshalTianpeng']
for bid in boss_ids:
    c = chars[bid]
    st = c['stats']
    print(f"  [{c['rare']}] {bid:<25} (Role: {c['type']:<8}): CritRate = {st['crit_rate']}% | CritRes = {st['crit_dmg_res']}% | BaseHP = {st['hp']:,} | BaseDEF = {st['def']:,}")

print("\n=== CHECK CHỈ SỐ LÍNH (CREEP SCALE x5.0) ===")
creep_ids = ['CrabSolider', 'YoungBufflalo', 'HeavenlySoddier', 'RatMonster', 'SquidSolider']
for cid in creep_ids:
    c = chars[cid]
    st = c['stats']
    lvl = 15
    hp_15 = st['hp'] + lvl * round(st['hp'] * 0.05)
    print(f"  {cid:<25} (Role: {c['type']:<8}): BaseHP = {st['hp']:,} | Lv 15 HP = {hp_15:,} | BaseDEF = {st['def']:,}")
