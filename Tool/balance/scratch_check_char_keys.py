import json

with open('Assets/Data/GameConfig/CharacterConfig.json', 'r', encoding='utf-8') as f:
    chars = json.load(f)

for k, v in list(chars.items())[:5]:
    print(k, list(v.keys()))
    if 'SunWukong' in k or 'Erlang' in k or 'Nezha' in k:
        print(v)
