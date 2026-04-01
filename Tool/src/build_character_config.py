import pandas as pd


import config
from src.build import BaseBuilder
from src.character_level_config_model import CostItem, TierData, AscensionTemplate, ExpCurveConfig

class CharacterUpdateLeveConfig(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    def run(self):
        print(f"Processing level config: {self.file_path}")

        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

        # Build AscensionTemplate
        ascension_templates = {}
        if "AscensionConfig" in all_sheets:
            df_ascension = all_sheets["AscensionConfig"]
            for _, row in df_ascension.iterrows():
                if pd.isna(row['ID']): continue
                template_id = str(row['ID']).strip()
                tier = str(row['Tier'])
                
                # 🌟 1. Khởi tạo bằng Data Class AscensionTemplate thay vì dict {}
                if template_id not in ascension_templates:
                    ascension_templates[template_id] = AscensionTemplate()
                
                # Trỏ vào object template hiện tại để dễ thao tác
                current_template = ascension_templates[template_id]
                
                # Cập nhật Max Tier
                current_tier_int = int(row['Tier'])
                if current_tier_int > current_template.max_tier:
                    current_template.max_tier = current_tier_int

                # Gom tài nguyên yêu cầu (Cost) vào một mảng
                cost_list = []

                # 🌟 2. Add Vàng bằng Data Class CostItem
                gold_cost = int(row['Cost_Gold'])
                if gold_cost > 0:
                    cost_list.append(CostItem(id="gold", quantity=gold_cost))

                # 🌟 3. Add Item_01 bằng Data Class CostItem
                if pd.notna(row['ItemID_01']):
                    item_id = str(row['ItemID_01']).strip()
                    item_qty = int(row['Item_01_Qty'])
                    if item_id != "" and item_qty > 0:
                        cost_list.append(CostItem(id=item_id, quantity=item_qty))
                
                # 🌟 4. Đóng gói Data bằng TierData
                # (Lưu ý: Bạn có khai báo thuộc tính 'max: int' trong TierData ở file model, tôi đang tạm để giá trị là 1)
                tier_data = TierData(
                    unlock_max_level=int(row['Unlock_Max_Level']),
                    level_req=int(row['Required_Level']),
                    max=1, 
                    cost=cost_list
                )

                # Nhét Data Tier này vào đúng ID của nó
                current_template.tier[tier] = tier_data
            
        # Lúc này 'item' thực sự là object của AscensionTemplate, nên gọi to_dict() vô tư!
        final_data = {item_id: item.to_dict() for item_id, item in ascension_templates.items()}
        
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_data, "AscensionConfig")

        # Build Level config

        print(f"Precessing level config: {self.file_path}")

        exp_curves = {}

        if "ExpCurvesConfig" in all_sheets:
            df_levels = all_sheets["ExpCurvesConfig"]

            curve_ids = [col for col in df_levels.columns if col.startswith("Curve_") and not col.endswith("_up")]
        

            for curve_id in curve_ids:
                    exp_curves[curve_id] = ExpCurveConfig()

            for _, row in df_levels.iterrows():
                if pd.isna(row['Level']): continue

                # Ép kiểu Level về số nguyên rồi thành chuỗi để làm Key JSON (vd: "1", "2")
                level_str = str(int(row['Level']))
                
                for curve_id in curve_ids:
                    # 1. Đọc cột Tổng EXP (vd: cột Curve_R)
                    if pd.notna(row[curve_id]):
                        exp_curves[curve_id].total_exp[level_str] = int(row[curve_id])
                        
                    # 2. Đọc cột EXP Lên Cấp (vd: cột Curve_R_up)
                    up_col_name = f"{curve_id}_up"
                    if up_col_name in df_levels.columns and pd.notna(row[up_col_name]):
                        exp_curves[curve_id].up_exp[level_str] = int(row[up_col_name])

            # Đóng gói và Xuất file JSON
            final_level_data = {curve_id: curve.to_dict() for curve_id, curve in exp_curves.items()}
            self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_level_data, "ExpCurvesConfig")
