import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

print("=== LiJing CharacterConfig ===")
print(json.dumps(chars.get('LiJing', {}).get('skills', {}), indent=2))

with open('Assets/Data/GameConfig/PassiveConfig.json', 'r', encoding='utf-8') as f:
    passives = json.load(f)

print("\n=== Passives related to LiJing ===")
for pid, pdata in passives.items():
    if 'lijing' in pid:
        print(f"[{pid}]: {json.dumps(pdata, indent=2)}")
