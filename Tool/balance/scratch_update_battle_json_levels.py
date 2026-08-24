import json, openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

# New level mapping based on smooth narrative progression (Max Campaign Boss Lv 35)
level_updates = {
    # Tutorial
    ('Tutorial_Default', 1): 2,
    ('Tutorial_Default', 2): 2,
    ('Tutorial_Default', 3): 2,
    
    # Region 1: Dong Hai
    ('Battle_GoldfishDemon', 1): 6,
    ('Battle_GoldfishDemon', 2): 6,
    ('Battle_GoldfishDemon', 3): 6,
    ('Battle_GoldfishDemon', 5): 8,
    
    ('Battle_ThirdDragonPrince', 1): 10,
    ('Battle_ThirdDragonPrince', 2): 10,
    ('Battle_ThirdDragonPrince', 3): 10,
    ('Battle_ThirdDragonPrince', 5): 12,
    
    # Region 2: Thien Dinh
    ('Battle_LiJing_Boss', 1): 15, # MarshalTianpeng
    ('Battle_LiJing_Boss', 2): 16, # LiJing_Boss
    ('Battle_LiJing_Boss', 3): 14, # HeavenlySoddier
    ('Battle_LiJing_Boss', 4): 14, # HeavenlySoddier
    ('Battle_LiJing_Boss', 5): 15, # ThirdPrinceNezha_Boss
    ('Battle_LiJing_Boss', 6): 14, # HeavenlySoddier
    
    ('Battle_ErlangShen_Boss', 1): 18,
    ('Battle_ErlangShen_Boss', 2): 18,
    ('Battle_ErlangShen_Boss', 3): 18,
    ('Battle_ErlangShen_Boss', 5): 20, # ErlangShen_Boss
    
    # Region 3: Cao Lao Trang & Hoang Phong
    ('Battle_YellowWindMonster', 1): 22,
    ('Battle_YellowWindMonster', 2): 22,
    ('Battle_YellowWindMonster', 3): 22,
    ('Battle_YellowWindMonster', 5): 24, # YellowWindMonster
    
    ('Battle_GoldHornKing', 1): 25,
    ('Battle_GoldHornKing', 2): 25,
    ('Battle_GoldHornKing', 3): 25,
    ('Battle_GoldHornKing', 4): 26, # SilverHornKing
    ('Battle_GoldHornKing', 5): 28, # GoldHornKing
    
    # Region 4: Hoa Diem Son
    ('Battle_TheBoySage', 1): 30,
    ('Battle_TheBoySage', 2): 30,
    ('Battle_TheBoySage', 3): 30,
    ('Battle_TheBoySage', 5): 32, # TheBoySage
    
    ('Battle_BullDemonKing', 1): 32,
    ('Battle_BullDemonKing', 2): 35, # BullDemonKing_Boss
    ('Battle_BullDemonKing', 3): 32,
    ('Battle_BullDemonKing', 4): 32,
    ('Battle_BullDemonKing', 5): 33, # TheBoySage
    ('Battle_BullDemonKing', 6): 32,
}

# Update BattleConfig.json directly
with open('Assets/Data/GameConfig/BattleConfig.json', 'r', encoding='utf-8') as f:
    battles = json.load(f)

for b_id, b_data in battles.items():
    for enemy in b_data.get('enemies', []):
        slot = enemy.get('slot')
        key = (b_id, slot)
        if key in level_updates:
            old_lvl = enemy.get('enemy_level')
            new_lvl = level_updates[key]
            enemy['enemy_level'] = new_lvl
            print(f"JSON: {b_id} slot {slot}: {old_lvl} -> {new_lvl}")

with open('Assets/Data/GameConfig/BattleConfig.json', 'w', encoding='utf-8') as f:
    json.dump(battles, f, indent=4, ensure_ascii=False)
print("Successfully updated BattleConfig.json!")
