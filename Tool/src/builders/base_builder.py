import pandas as pd
import mmh3
import json
import os

class BaseBuilder:
    def get_hash(self, key):
        if pd.isna(key) or str(key).strip() == "": return 0
        return mmh3.hash(str(key).strip(), signed=False)
        
    def export_json(self, folder, data, filename):
        path = os.path.join(folder, f"{filename}.json")
        with open(path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=4, ensure_ascii=False)
