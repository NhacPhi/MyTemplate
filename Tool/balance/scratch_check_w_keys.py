import json

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    player_data = json.load(f)

for w in player_data['inventory']['weapons'][:3]:
    print(w)
