import json

with open('Assets/Data/GameConfig/ItemConfig.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

for k, v in data.items():
    if v.get('type') == 'Armor':
        adata = v.get('armor_data', {})
        print(f"{k:<22} Part={adata.get('part'):<12} MainStat={adata.get('main_stat')}")
