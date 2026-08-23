import json

with open('Assets/Data/GameConfig/ShopConfig.json', 'r', encoding='utf-8') as f:
    shop = json.load(f)

for pid, pdata in shop.items():
    if 'ARMOR' in pid:
        print(f"Product: {pid}")
        for c in pdata.get('bundle_contents', []):
            print(f"  -> {c.get('item_id')}")
