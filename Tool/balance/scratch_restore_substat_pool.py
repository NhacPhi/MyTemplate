import openpyxl, sys
sys.stdout.reconfigure(encoding='utf-8')

wb = openpyxl.load_workbook('Tool/data/GameConfig.xlsx')
ws = wb['SubstatPool']

# Restore SubstatPool exactly to original values
original_pool_values = {
    ('Warrior_Pool', 'HP', 'Flat'): (100, 500),
    ('Warrior_Pool', 'ATK', 'Flat'): (15, 75),
    ('Warrior_Pool', 'DEF', 'Flat'): (10, 50),
    ('Warrior_Pool', 'HP', 'Percent'): (2, 6),
    ('Warrior_Pool', 'ATK', 'Percent'): (2, 6),
    ('Warrior_Pool', 'DEF', 'Percent'): (2, 6),
    ('Warrior_Pool', 'PENETRATION', 'Percent'): (3, 8),
    ('Warrior_Pool', 'CRIT_RATE', 'Percent'): (2, 5),
    ('Warrior_Pool', 'CRIT_DMG', 'Percent'): (5, 12),
    ('Warrior_Pool', 'DEF_SHRED', 'Flat'): (8, 24),
    ('Warrior_Pool', 'SPEED', 'Flat'): (1, 4),
    ('Warrior_Pool', 'CRIT_DMG_RES', 'Percent'): (2, 5),
    ('Tanker_Pool', 'HP', 'Flat'): (200, 800),
    ('Tanker_Pool', 'DEF', 'Flat'): (20, 80),
    ('Tanker_Pool', 'HP', 'Percent'): (3, 8),
    ('Tanker_Pool', 'DEF', 'Percent'): (3, 8),
    ('Tanker_Pool', 'CRIT_DMG_RES', 'Percent'): (2, 5),
    ('Tanker_Pool', 'SPEED', 'Flat'): (1, 3),
    ('Assassin_Pool', 'ATK', 'Flat'): (25, 100),
    ('Assassin_Pool', 'ATK', 'Percent'): (3, 7),
    ('Assassin_Pool', 'DEF_SHRED', 'Flat'): (15, 40),
    ('Assassin_Pool', 'PENETRATION', 'Percent'): (4, 10),
    ('Assassin_Pool', 'CRIT_RATE', 'Percent'): (3, 7),
    ('Assassin_Pool', 'CRIT_DMG', 'Percent'): (8, 18),
    ('Assassin_Pool', 'SPEED', 'Flat'): (2, 5)
}

for row in ws.iter_rows(min_row=3):
    pool_id = row[0].value
    stat_type = row[1].value
    mod_type = row[4].value if len(row) > 4 else None
    
    key = (pool_id, stat_type, mod_type)
    if key in original_pool_values:
        min_v, max_v = original_pool_values[key]
        row[2].value = float(min_v)
        row[3].value = float(max_v)
        print(f"Restored {pool_id} - {stat_type} ({mod_type}) => Min={min_v}, Max={max_v}")

wb.save('Tool/data/GameConfig.xlsx')
print("Successfully restored SubstatPool in Tool/data/GameConfig.xlsx")
