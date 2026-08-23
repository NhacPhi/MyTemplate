import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

print(f"{'ID':<25} {'Rare':<6} {'Role':<10} {'HP':<10} {'ATK':<10} {'DEF':<10} {'CRIT_RES':<10}")
print("-" * 80)

sample_ids = [
    'SunWukong', 'ErlangShen', 'ThirdPrinceNezha', 'BullDemonKing', # Playable
    'CrabSolider', 'YoungBufflalo', 'HeavenlySoddier',               # Creeps
    'GoldfishDemon', 'ThirdDragonPrince', 'BullDemonKing_Boss', 'ErlangShen_Boss' # Bosses
]

for cid in sample_ids:
    c = chars.get(cid)
    if not c: continue
    st = c['stats']
    print(f"{cid:<25} {c.get('rare',''):<6} {c.get('type',''):<10} {st.get('hp',0):<10} {st.get('atk',0):<10} {st.get('def',0):<10} {st.get('crit_dmg_res',0):<10}")
