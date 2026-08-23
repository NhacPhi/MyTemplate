import json, openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

for cid in ['SunWukong', 'ErlangShen', 'ThirdPrinceNezha']:
    c = chars[cid]
    print(f"=== {cid} ({c['rare']} {c['type']}) ===")
    print("Stats:", c['stats'])
    print("Skills:")
    for sk_name, sk_data in c['skills'].items():
        print(f"  [{sk_name}] ID={sk_data['id']}, Type={sk_data['skill_type']}, Target={sk_data['target_type']}, Mult={sk_data['damage_multiplier']}, CD={sk_data['max_cooldown']}, Effect={sk_data['effect_id']}")
    print()
