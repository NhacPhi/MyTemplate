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
                name_val = row['Name'] if pd.notna(row.get('Name')) else ""
                pieces_val = int(row['Pieces_Required']) if pd.notna(row.get('Pieces_Required')) else 6
                
                raw_stats = [s.strip() for s in str(row['Bonus_Stat_Type']).split(',')] if pd.notna(row.get('Bonus_Stat_Type')) else [""]
                raw_values = [v.strip() for v in str(row['Bonus_Value']).split(',')] if pd.notna(row.get('Bonus_Value')) else ["0"]
                raw_mods = [m.strip() for m in str(row['Modifier_Type']).split(',')] if pd.notna(row.get('Modifier_Type')) else ["Flat"]

                entries = []
                for i in range(len(raw_stats)):
                    s_name = raw_stats[i].strip()
                    if not s_name: continue
                    try:
                        v_val = float(raw_values[i].strip()) if i < len(raw_values) else float(raw_values[0].strip())
                    except ValueError:
                        v_val = 0.0
                    m_val = raw_mods[i].strip() if i < len(raw_mods) else raw_mods[0].strip()
                    entries.append({
                        "stat": s_name,
                        "value": v_val,
                        "modifier_type": m_val
                    })

                if set_id in set_bonus_configs:
                    existing_model = set_bonus_configs[set_id]
                    if existing_model.stats is None:
                        existing_model.stats = [{
                            "stat": existing_model.stat,
                            "value": existing_model.value,
                            "modifier_type": existing_model.modifier_type
                        }]
                    existing_model.stats.extend(entries)
                else:
                    first_entry = entries[0] if entries else {"stat": "", "value": 0.0, "modifier_type": "Flat"}
                    set_bonus_configs[set_id] = SetBonusModel(
                        name_hash=self.get_hash(name_val),
                        pieces=pieces_val,
                        stat=first_entry["stat"],
                        value=first_entry["value"],
                        modifier_type=first_entry["modifier_type"],
                        stats=entries if len(entries) > 1 else None
                    )


            final_data = {item_id: item.to_dict() for item_id, item in set_bonus_configs.items()}
            # Export dữ liệu ra JSON
            self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_data, "SetBonusConfig")
            print("Successfully exported SetBonusConfig.json")
        else:
            print(f"Sheet '{sheet_name}' not found in the excel file.")