import json, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Data/Player.json', 'r', encoding='utf-8') as f:
    player_data = json.load(f)

chars = player_data.get('roster', {}).get('characters', [])
for c in chars:
    print(c)
