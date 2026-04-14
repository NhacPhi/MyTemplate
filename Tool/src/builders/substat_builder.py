import pandas as pd
import config
from src.builders.base_builder import BaseBuilder
from src.models.substat_models import SetBonusModel

class SubstatPoolBuilder(BaseBuilder):
    def __init__(self, file_path):
        super().__init__()
        self.file_path = file_path

    def run(self):
        print(f"Processing Substat Pool config: {self.file_path}")

        try:
            # Đọc toàn bộ các sheet
            all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        except Exception as e:
            print(f"Failed to read {self.file_path}: {e}")
            return

        # Dictionary lưu trữ kết quả cuối cùng
        # Cấu trúc: { "Warrior_Pool": [SetBonusModel, ...], ... }
        pool_configs = {}

        sheet_name = "SubstatPool" 
        if sheet_name in all_sheets:
            df = all_sheets[sheet_name]
            
            for _, row in df.iterrows():
                # Bỏ qua dòng nếu ID trống
                if pd.isna(row.get('ID')): 
                    continue
                
                pool_id = str(row['ID']).strip()
                
                # Khởi tạo danh sách nếu PoolID này chưa tồn tại
                if pool_id not in pool_configs:
                    pool_configs[pool_id] = { "Pools": [] }
                
                # Tạo model cho từng dòng chỉ số
                stat_data = SetBonusModel(
                    stat_type=str(row['StatType']).strip(),
                    min=float(row['Min']) if pd.notna(row['Min']) else 0.0,
                    max=float(row['Max']) if pd.notna(row['Max']) else 0.0,
                    modifier_type=str(row['Modifier_Type']).strip() if pd.notna(row['Modifier_Type']) else ""
                )
                
                # Thêm vào danh sách của PoolID tương ứng
                pool_configs[pool_id]["Pools"].append(stat_data.to_dict())

            # Export dữ liệu ra JSON
            # Kết quả sẽ có dạng: {"Warrior_Pool": [{"type": "HP", "min": 100, ...}, {...}]}
            self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, pool_configs, "SubstatPoolConfig")
            print("Successfully exported SubstatPoolConfig.json")
        else:
            print(f"Sheet '{sheet_name}' not found in the excel file.")