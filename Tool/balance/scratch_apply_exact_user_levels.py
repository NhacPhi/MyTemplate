import json, openpyxl, time, sys
sys.stdout.reconfigure(encoding='utf-8')

# Precise level specs requested by user:
# Tutorial: lv 5
# Vung 1:
# Battle_GoldfishDemon: Linh Ca Lv 10, Boss GoldfishDemon Lv 12
# Battle_ThirdDragonPrince: Linh Cua/Muc Lv 15, Boss ThirdDragonPrince Lv 20, GoldfishDemon Lv 20
# Vung 2:
# Battle_LiJing_Boss: Thien Binh Lv 20, Na Tra/Bat Gioi Lv 20, Boss LiJing Lv 25
# Battle_ErlangShen_Boss: Thien Binh Lv 20, Boss ErlangShen Lv 25
# Vung 3:
# Battle_YellowWindMonster: Linh Ho/Chuot Lv 20, Boss YellowWindMonster Lv 28
# Battle_GoldHornKing: Yeu Binh Lv 25, Ngan Giac Lv 26, Boss Kim Giac Lv 28
# Vung 4:
# Battle_TheBoySage: Bo Ma Lv 25, Boss TheBoySage Lv 30
# Battle_BullDemonKing: Bo Ma Lv 25, Hong Hai Nhi Lv 30, Boss BullDemonKing Lv 30

exact_levels = {
    # Tutorial
    ('Tutorial_Default', 1): ('VanguardTiger', 5, False),
    ('Tutorial_Default', 2): ('VanguardTiger', 5, False),
    ('Tutorial_Default', 3): ('VanguardTiger', 5, False),
    
    # Region 1: Dong Hai
    ('Battle_GoldfishDemon', 1): ('Benborba', 10, False),
    ('Battle_GoldfishDemon', 2): ('Baborben', 10, False),
    ('Battle_GoldfishDemon', 3): ('Benborba', 10, False),
    ('Battle_GoldfishDemon', 5): ('GoldfishDemon', 12, True),
    
    ('Battle_ThirdDragonPrince', 1): ('CrabSolider', 15, False),
    ('Battle_ThirdDragonPrince', 2): ('SquidSolider', 15, False),
    ('Battle_ThirdDragonPrince', 3): ('CrabSolider', 15, False),
    ('Battle_ThirdDragonPrince', 4): ('GoldfishDemon', 20, False),
    ('Battle_ThirdDragonPrince', 5): ('ThirdDragonPrince', 20, True),
    
    # Region 2: Thien Dinh
    ('Battle_LiJing_Boss', 1): ('MarshalTianpeng', 20, False),
    ('Battle_LiJing_Boss', 2): ('LiJing_Boss', 25, True),
    ('Battle_LiJing_Boss', 3): ('HeavenlySoddier', 20, False),
    ('Battle_LiJing_Boss', 4): ('HeavenlySoddier', 20, False),
    ('Battle_LiJing_Boss', 5): ('ThirdPrinceNezha_Boss', 20, False),
    ('Battle_LiJing_Boss', 6): ('HeavenlySoddier', 20, False),
    
    ('Battle_ErlangShen_Boss', 1): ('HeavenlySoddier', 20, False),
    ('Battle_ErlangShen_Boss', 2): ('HeavenlySoddier', 20, False),
    ('Battle_ErlangShen_Boss', 3): ('HeavenlySoddier', 20, False),
    ('Battle_ErlangShen_Boss', 5): ('ErlangShen_Boss', 25, True),
    
    # Region 3: Cao Lao Trang & Hoang Phong
    ('Battle_YellowWindMonster', 1): ('VanguardTiger', 20, False),
    ('Battle_YellowWindMonster', 2): ('RatMonster', 20, False),
    ('Battle_YellowWindMonster', 3): ('VanguardTiger', 20, False),
    ('Battle_YellowWindMonster', 5): ('YellowWindMonster', 28, True),
    
    ('Battle_GoldHornKing', 1): ('Minions', 25, False),
    ('Battle_GoldHornKing', 2): ('Minions', 25, False),
    ('Battle_GoldHornKing', 3): ('Minions', 25, False),
    ('Battle_GoldHornKing', 4): ('SilverHornKing', 26, False),
    ('Battle_GoldHornKing', 5): ('GoldHornKing', 28, True),
    
    # Region 4: Hoa Diem Son
    ('Battle_TheBoySage', 1): ('YoungBufflalo', 25, False),
    ('Battle_TheBoySage', 2): ('YoungBufflalo', 25, False),
    ('Battle_TheBoySage', 3): ('YoungBufflalo', 25, False),
    ('Battle_TheBoySage', 5): ('TheBoySage', 30, True),
    
    ('Battle_BullDemonKing', 1): ('YoungBufflalo', 25, False),
    ('Battle_BullDemonKing', 2): ('BullDemonKing_Boss', 30, True),
    ('Battle_BullDemonKing', 3): ('YoungBufflalo', 25, False),
    ('Battle_BullDemonKing', 4): ('YoungBufflalo', 25, False),
    ('Battle_BullDemonKing', 5): ('TheBoySage', 30, False),
    ('Battle_BullDemonKing', 6): ('YoungBufflalo', 25, False),
}

# 1. Update BattleConfig.json
with open('Assets/Data/GameConfig/BattleConfig.json', 'r', encoding='utf-8') as f:
    battles = json.load(f)

for b_id, b_data in battles.items():
    new_enemies = []
    # Check all slots
    slots_for_battle = [slot for (bid, slot) in exact_levels if bid == b_id]
    slots_for_battle.sort()
    
    for slot in slots_for_battle:
        enemy_id, lvl, is_boss = exact_levels[(b_id, slot)]
        new_enemies.append({
            "slot": slot,
            "enemy_id": enemy_id,
            "enemy_level": lvl,
            "boss": is_boss
        })
    b_data["enemies"] = new_enemies

with open('Assets/Data/GameConfig/BattleConfig.json', 'w', encoding='utf-8') as f:
    json.dump(battles, f, indent=4, ensure_ascii=False)
print("Updated BattleConfig.json with exact user levels!")

# 2. Update GameConfig.xlsx StageEnemies sheet
for attempt in range(5):
    try:
        wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
        ws = wb['StageEnemies']
        
        # Clear existing rows except header
        while ws.max_row > 1:
            ws.delete_rows(2)
            
        for (b_id, slot), (enemy_id, lvl, is_boss) in exact_levels.items():
            ws.append([b_id, slot, enemy_id, lvl, is_boss])
            
        wb.save('Tool/data/GameConfig.xlsx')
        print("Updated GameConfig.xlsx StageEnemies sheet successfully!")
        break
    except PermissionError:
        print(f"Excel locked, attempt {attempt+1}, retrying in 1s...")
        time.sleep(1)
