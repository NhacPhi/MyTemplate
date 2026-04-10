import pandas as pd
import config
from src.builders.base_builder import BaseBuilder

# Giả định đường dẫn đến model của bạn
from src.models.passive_skill_models import PassiveSkillModel

class PassiveSkillBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing PassiveSkill config: {self.file_path}")

        try:
            # Đọc toàn bộ các sheet
            all_sheets = pd.read_excel(self.file_path, sheet_name=None)
        except Exception as e:
            print(f"Failed to read {self.file_path}: {e}")
            return

        passive_configs = {}

        # Tên sheet cấu hình Nội tại
        sheet_name = "PassiveConfig" 
        
        if sheet_name in all_sheets:
            df = all_sheets[sheet_name]
            
            for _, row in df.iterrows():
                # Bỏ qua dòng nếu cột khóa chính 'id' bị trống
                if pd.isna(row.get('ID')): 
                    continue
                
                skill_id = str(row['ID']).strip()
                

                desc_hash = self.get_hash(str(row['desc_template'])) if pd.notna(row.get('desc_template')) else 0
                
                p0_raw = str(row['param0_values']) if pd.notna(row.get('param0_values')) else ""
                p1_raw = str(row['param1_values']) if pd.notna(row.get('param1_values')) else ""

                # 3. Khởi tạo Model 
                # (Nhờ hàm __post_init__ trong Model, chuỗi p0_raw sẽ tự động biến thành List[float])
                passive_configs[skill_id] = PassiveSkillModel(
                    desc_template_hash=desc_hash,
                    param0_values=p0_raw,
                    param1_values=p1_raw
                )

            # Chuyển đổi toàn bộ Dictionary các Object Model thành Dictionary tiêu chuẩn để xuất JSON
            final_data = {item_id: item.to_dict() for item_id, item in passive_configs.items()}
            
            # Export dữ liệu ra JSON bằng hàm của BaseBuilder
            self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_data, "PassiveConfig")
            print("Successfully exported PassiveConfig.json")
            
        else:
            print(f"Sheet '{sheet_name}' not found in the excel file.")