import pandas as pd
import config
from src.builders.base_builder import BaseBuilder
from src.models.set_bonus_model import SetBonusModel # Giả định đường dẫn model của bạn

class SetBonusBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing SetBonus config: {self.file_path}")

        try:
            # Đọc toàn bộ các sheet
            all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        except Exception as e:
            print(f"Failed to read {self.file_path}: {e}")
            return

        # Dictionary lưu trữ kết quả cuối cùng
        # Cấu trúc: { "Armor01": [SetBonusModel_Dict, ...], "Armor02": [...] }
        set_bonus_configs = {}

        # Giả định dữ liệu nằm trong sheet "SetBonus" hoặc tên sheet tương ứng của bạn
        sheet_name = "SetBonusConfig" 
        if sheet_name in all_sheets:
            df = all_sheets[sheet_name]
            
            for _, row in df.iterrows():
                # Bỏ qua dòng nếu SetArmor trống
                if pd.isna(row.get('ID')): 
                    continue
                
                set_id = str(row['ID']).strip()
                
                # Tạo model từ dòng dữ liệu
                set_bonus_configs[set_id] = SetBonusModel(
                    name_hash=self.get_hash(row['Name']),
                    pieces=int(row['Pieces_Required']) if pd.notna(row['Pieces_Required']) else 0,
                    stat=str(row['Bonus_Stat_Type']).strip() if pd.notna(row['Bonus_Stat_Type']) else "",
                    value=float(row['Bonus_Value']) if pd.notna(row['Bonus_Value']) else 0.0,
                    modifier_type=str(row['Modifier_Type']).strip() if pd.notna(row['Modifier_Type']) else "Constant"
                )


            final_data = {item_id: item.to_dict() for item_id, item in set_bonus_configs.items()}
            # Export dữ liệu ra JSON
            self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_data, "SetBonusConfig")
            print("Successfully exported SetBonusConfig.json")
        else:
            print(f"Sheet '{sheet_name}' not found in the excel file.")