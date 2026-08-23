import random, sys
sys.stdout.reconfigure(encoding='utf-8')

def get_main_stat(part):
    if part == 'Helmet': return 'HP'
    elif part == 'Chestplate': return 'DEF'
    elif part == 'Gloves': return 'ATK'
    elif part == 'Boots': return random.choice(['SPEED', 'ATK', 'HP', 'DEF'])
    elif part == 'Belt': return random.choice(['ATK', 'HP', 'DEF', 'PENETRATION'])
    elif part == 'Ring': return random.choice(['CRIT_RATE', 'CRIT_DMG', 'ATK', 'HP'])

print("=== MUA GÓI A (Nón, Áo, Tay) x 3 lần ===")
for i in range(3):
    print(f"Lần {i+1}: Nón={get_main_stat('Helmet')}, Áo={get_main_stat('Chestplate')}, Tay={get_main_stat('Gloves')}")

print("\n=== MUA GÓI B (Giày, Thắt lưng, Nhẫn) x 3 lần ===")
for i in range(3):
    print(f"Lần {i+1}: Giày={get_main_stat('Boots')}, Thắt lưng={get_main_stat('Belt')}, Nhẫn={get_main_stat('Ring')}")
