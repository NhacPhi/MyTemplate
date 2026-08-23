import openpyxl
import time
import sys

sys.stdout.reconfigure(encoding='utf-8')

gc_path = 'Tool/data/GameConfig.xlsx'

for attempt in range(5):
    try:
        wb = openpyxl.load_workbook(gc_path)
        ws = wb['SubstatPool']
        
        for row in ws.iter_rows(values_only=False):
            pool_id = row[0].value
            stat_type = row[1].value
            
            if pool_id == 'Warrior_Pool' and stat_type == 'DEF_SHRED':
                row[2].value = 8
                row[3].value = 24
                print(f"Updated {pool_id} DEF_SHRED to Min=8, Max=24")
                
            elif pool_id == 'Assassin_Pool' and stat_type == 'DEF_SHRED':
                row[2].value = 15
                row[3].value = 40
                print(f"Updated {pool_id} DEF_SHRED to Min=15, Max=40")

        wb.save(gc_path)
        print("Successfully saved GameConfig.xlsx with balanced DEF_SHRED!")
        break
    except Exception as e:
        print(f"Attempt {attempt+1} failed: {e}. Retrying...")
        time.sleep(1)
