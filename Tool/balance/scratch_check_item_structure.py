import json

with open('Assets/Data/GameConfig/ItemConfig.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

for k, v in list(data.items())[:15]:
    print(k, "=> type:", v.get("item_type") or v.get("type"), "keys:", list(v.keys()))
