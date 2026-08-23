import json

with open('Assets/Data/GameConfig/ItemConfig.json', 'r', encoding='utf-8') as f:
    item_cfg = json.load(f)

for k, v in item_cfg.items():
    if 'Armor' in k:
        armor_data = v.get('armor_data', {})
        print(f"{k:<20}: Part={armor_data.get('part')}, MainStat={armor_data.get('main_stat')}")
