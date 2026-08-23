import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

print(f"{'ID':<25} {'Class':<10} {'CRIT_RATE':<12} {'CRIT_DMG':<12} {'CRIT_RES':<12} {'PENETRATION':<14} {'HP (Base)':<12}")
print("=" * 97)

boss_list = [
    'GoldfishDemon', 'ThirdDragonPrince', 'BullDemonKing_Boss', 'ErlangShen_Boss', 
    'YellowWindMonster', 'TheBoySage', 'GoldHornKing', 'SilverHornKing', 
    'MarshalTianpeng', 'ThirdPrinceNezha_Boss', 'LiJing_Boss', 'VanguardTiger'
]

for bid in boss_list:
    c = chars.get(bid)
    if not c: continue
    st = c['stats']
    cd_total = 150 + st.get('crit_dmg', 0)
    print(f"{bid:<25} {'Boss':<10} {str(st.get('crit_rate'))+'%':<12} {str(cd_total)+'% ('+str(st.get('crit_dmg'))+')':<12} {str(st.get('crit_dmg_res'))+'%':<12} {str(st.get('penetration'))+'%':<14} {st.get('hp'):,}")

print("\n" + "=" * 97)
print("CREEP / MINION STATS (Scale x3.0):")
creep_list = ['CrabSolider', 'YoungBufflalo', 'HeavenlySoddier', 'RatMonster', 'SquidSolider', 'Minions']
for cid in creep_list:
    c = chars.get(cid)
    if not c: continue
    st = c['stats']
    print(f"{cid:<25} {'Creep':<10} {str(st.get('crit_rate'))+'%':<12} {str(st.get('crit_dmg'))+'%':<12} {str(st.get('crit_dmg_res'))+'%':<12} {str(st.get('penetration'))+'%':<14} {st.get('hp'):,}")
