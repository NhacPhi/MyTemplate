import json, openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    player_data = json.load(f)

print("=== CHECK PENETRATION & DEF_SHRED TRÊN TRANG BỊ CỦA ROSTER ===")
for c in player_data['roster']['characters']:
    cid = c['id']
    armors_dict = {a.get('instance_id') or a.get('uuid'): a for a in player_data['inventory']['armors']}
    total_pen = 0
    total_shred = 0
    print(f"\n--- {cid} ---")
    for eq in c.get('armors', []):
        a = armors_dict.get(eq['id'])
        if not a: continue
        print(f"  {eq['type']}: {a['template_id']} | Main: {a.get('main_stat_type')} | Substats: {a.get('substats')}")
