import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    player_data = json.load(f)

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    char_configs = json.load(f)

with open('Assets/Data/GameConfig/ItemConfig.json', 'r', encoding='utf-8') as f:
    item_configs = json.load(f)

weapons_dict = {w.get('uuid') or w.get('instance_id'): w for w in player_data['inventory']['weapons']}
armors_dict = {a.get('instance_id') or a.get('uuid'): a for a in player_data['inventory']['armors']}

for c in player_data['roster']['characters']:
    cid = c['id']
    if cid not in ['SunWukong', 'ErlangShen', 'ThirdPrinceNezha', 'BullDemonKing']:
        continue
    
    cfg = char_configs[cid]
    print(f"\n==================== {cid} (Rare: {cfg['rare']}, Role: {cfg['type']}, Lv: {c['level']}) ====================")
    w_inst = weapons_dict.get(c.get('weapon'))
    if w_inst:
        print(f"  Weapon: {w_inst['template_id']} (Lv {w_inst.get('current_level', 1)})")
    
    c_armors = c.get('armors', [])
    print(f"  Equipped Armors ({len(c_armors)}):")
    for eq in c_armors:
        a_inst = armors_dict.get(eq['id'])
        if a_inst:
            print(f"    - {eq['type']}: {a_inst['template_id']} (Lv {a_inst.get('level')}, Rare {a_inst.get('rare')}, MainStat: {a_inst.get('main_stat_type')})")
            print(f"        Substats: {a_inst.get('substats')}")
