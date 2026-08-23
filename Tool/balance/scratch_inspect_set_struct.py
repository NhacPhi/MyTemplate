import json

with open('Assets/Data/GameConfig/SetBonusConfig.json', 'r', encoding='utf-8') as f:
    set_bonuses = json.load(f)

for k, v in set_bonuses.items():
    print(k, "=>", v)
