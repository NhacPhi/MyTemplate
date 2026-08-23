import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

print(f"{'ID':<25} {'Class':<10} {'Rare':<6} {'Role':<10} {'CRIT_RATE':<12} {'CRIT_DMG_RES':<14} {'HP':<10}")
print("-" * 90)

check_ids = [
    'SunWukong', 'ErlangShen', 'ThirdPrinceNezha', 'BullDemonKing', # Playable
    'VanguardTiger', 'TheBoySage', 'GoldHornKing', 'SilverHornKing', 'YellowWindMonster', 
    'GoldfishDemon', 'MarshalTianpeng', 'ThirdPrinceNezha_Boss', 'BullDemonKing_Boss', 
    'ErlangShen_Boss', 'LiJing_Boss', 'ThirdDragonPrince' # Bosses
]

for cid in check_ids:
    c = chars.get(cid)
    if not c: continue
    st = c['stats']
    cls = 'Boss' if 'Boss' in cid or cid in ['VanguardTiger', 'TheBoySage', 'GoldHornKing', 'SilverHornKing', 'YellowWindMonster', 'GoldfishDemon', 'MarshalTianpeng', 'ThirdDragonPrince'] else 'Character'
    print(f"{cid:<25} {cls:<10} {c.get('rare',''):<6} {c.get('type',''):<10} {str(st.get('crit_rate',0))+'%':<12} {str(st.get('crit_dmg_res',0))+'%':<14} {st.get('hp',0):<10}")
