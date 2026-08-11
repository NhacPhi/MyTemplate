import pandas as pd
import config
from src.builders.base_builder import BaseBuilder
from src.models.character_models import CharacterModel, SkillComponent, AttributeComponent

class CharacterConfigBuilder(BaseBuilder):
    def __init__(self, file_path):
        self.file_path = file_path

    @staticmethod
    def _parse_array(value, cast_func):
        """Parse a comma-separated string like '1, 1.2' into a list, e.g. [1.0, 1.2]."""
        raw = str(value).strip()
        # Remove surrounding parentheses/brackets if present
        raw = raw.strip('()[]')
        return [cast_func(v.strip()) for v in raw.split(',') if v.strip()]

    def run(self):
        print(f"Processing character config: {self.file_path}")
        
        all_sheets = pd.read_excel(self.file_path, sheet_name=None)

        skill_lookup = {}
        if "SkillConfig" in all_sheets:
            df_skills = all_sheets["SkillConfig"]
            for _, row in df_skills.iterrows():
                if pd.isna(row['ID']): continue
            
                skill_id = str(row['ID']).strip()
                
                dm = self._parse_array(row['DamageMultiplier'], float) if not pd.isna(row['DamageMultiplier']) else [0.0]
                mc = [int(x) for x in self._parse_array(row['MaxCooldown'], float)] if not pd.isna(row['MaxCooldown']) else [0]

                # Tự động copy giá trị cuối cùng nếu mảng ngắn hơn DamageMultiplier
                if len(mc) < len(dm):
                    mc.extend([mc[-1]] * (len(dm) - len(mc)))

                skill_lookup[skill_id] = SkillComponent(
                    id=skill_id,
                    name_hash=self.get_hash((row['Name'].strip())) if not pd.isna(row['Name']) else 0,
                    des_hash=self.get_hash((row['Des']).strip()) if not pd.isna(row['Des']) else 0,
                    skill=str(row['Type']).strip() if not pd.isna(row['Type']) else "None",
                    skill_type=str(row['SkillType']).strip() if not pd.isna(row['SkillType']) else "None",
                    target_type=str(row['TargetType']).strip() if not pd.isna(row['TargetType']) else "None",
                    damage_multiplier=dm,
                    max_cooldown=mc,
                    flat_damage=float(row['FlatDamage']) if not pd.isna(row['FlatDamage']) else 0,
                    effect_id=str(row['Effect']).strip() if not pd.isna(row['Effect']) else "None",
                    passive_id=str(row['PassiveID']).strip() if 'PassiveID' in df_skills.columns and not pd.isna(row['PassiveID']) else "",
                    sound=str(row['Sound']).strip() if not pd.isna(row['Sound']) else ""
                )

        character_data = {}
        if "Character" in all_sheets:
            df = all_sheets["Character"]
            for _, row in df.iterrows():
                if pd.isna(row['ID']) : continue

                character_id = str(row['ID']).strip()

                base_id = str(row['Base']).strip() if not pd.isna(row['Base']) else None
                major_id = str(row['Major']).strip() if not pd.isna(row['Major']) else None
                ultimate_id = str(row['Ultimate']).strip() if not pd.isna(row['Ultimate']) else None

                skill_data = {}
                if base_id and base_id in skill_lookup:
                    skill_data["Base"] = skill_lookup[base_id]
                if major_id and major_id in skill_lookup:
                    skill_data["Major"] = skill_lookup[major_id]
                if ultimate_id and ultimate_id in skill_lookup:
                    skill_data["Ultimate"] = skill_lookup[ultimate_id]

                character_data[character_id] = CharacterModel(
                    name_hash=self.get_hash(row['Name']),
                    rare=str(row['Rare']),
                    type=str(row['Type']),
                    skills=skill_data
                )

        rarity_mult = {'R': 1.0, 'SR': 1.2, 'SSR': 1.45, 'UR': 1.7}
        rarity_tier = {'R': 0, 'SR': 1, 'SSR': 2, 'UR': 3}
        role_base_data = {
            'Tanker':   {'hp': 4000, 'atk': 900,  'def': 375, 'speed': 80,  'crit_rate': 5,  'crit_dmg': 0,  'def_shred': 0,  'penetration': 0, 'crit_dmg_res': 5},
            'Fighter':  {'hp': 2750, 'atk': 1650, 'def': 275, 'speed': 90,  'crit_rate': 5,  'crit_dmg': 5,  'def_shred': 5,  'penetration': 2, 'crit_dmg_res': 0},
            'Assassin': {'hp': 2000, 'atk': 2250, 'def': 200, 'speed': 105, 'crit_rate': 10, 'crit_dmg': 15, 'def_shred': 10, 'penetration': 5, 'crit_dmg_res': 0},
            'Mage':     {'hp': 2250, 'atk': 2100, 'def': 210, 'speed': 95,  'crit_rate': 5,  'crit_dmg': 10, 'def_shred': 0,  'penetration': 5, 'crit_dmg_res': 0},
            'Support':  {'hp': 3000, 'atk': 1050, 'def': 250, 'speed': 100, 'crit_rate': 5,  'crit_dmg': 0,  'def_shred': 0,  'penetration': 0, 'crit_dmg_res': 3},
            'ADCarry':  {'hp': 2200, 'atk': 2200, 'def': 220, 'speed': 100, 'crit_rate': 8,  'crit_dmg': 10, 'def_shred': 5,  'penetration': 4, 'crit_dmg_res': 0}
        }
        role_inc_data = {
            'Tanker':   {'speed': 2, 'crit_rate': 0, 'crit_dmg': 0,  'def_shred': 0, 'penetration': 0, 'crit_dmg_res': 2},
            'Fighter':  {'speed': 2, 'crit_rate': 1, 'crit_dmg': 3,  'def_shred': 2, 'penetration': 1, 'crit_dmg_res': 0},
            'Assassin': {'speed': 3, 'crit_rate': 2, 'crit_dmg': 3,  'def_shred': 3, 'penetration': 2, 'crit_dmg_res': 0},
            'Mage':     {'speed': 2, 'crit_rate': 1, 'crit_dmg': 3,  'def_shred': 0, 'penetration': 2, 'crit_dmg_res': 0},
            'Support':  {'speed': 3, 'crit_rate': 0, 'crit_dmg': 0,  'def_shred': 0, 'penetration': 0, 'crit_dmg_res': 2},
            'ADCarry':  {'speed': 3, 'crit_rate': 2, 'crit_dmg': 3,  'def_shred': 2, 'penetration': 1, 'crit_dmg_res': 0}
        }
        class_dict = {
            'Character': (1.0, 1.0, 1.0, 0),
            'Creep':     (0.7, 0.7, 0.7, 0),
            'Boss':      (1.8, 1.2, 1.2, 10)
        }
        bias_dict = {
            'Balanced':  (1.00, 1.00, 1.00,  0, 0, 0,  0, 0, 0),
            'Offensive': (0.95, 1.10, 0.95,  0, 2, 8,  5, 0, 0),
            'Defensive': (1.15, 0.90, 1.10,  0, 0, 0,  0, 0, 5),
            'Swift':     (0.95, 0.95, 0.95,  8, 5, 0,  0, 0, 0),
            'Heavy':     (1.00, 1.05, 1.05, -5, 0, 0, 15, 0, 0)
        }

        def parse_val(val):
            if pd.isna(val): return None
            if isinstance(val, (int, float)): return int(round(val))
            s = str(val).strip()
            if s.startswith('='): return None
            try: return int(round(float(s)))
            except: return None

        if "CharacterStat" in all_sheets:
            df = all_sheets["CharacterStat"]
            
            for _, row in df.iterrows():
                char_id = str(row['ID']).strip()
                if char_id.startswith('='):
                    continue
                if char_id in character_data:
                    rare_raw = str(row['Rare']).strip() if 'Rare' in df.columns and pd.notna(row['Rare']) else ''
                    role_raw = str(row['Type']).strip() if 'Type' in df.columns and pd.notna(row['Type']) else ''
                    cls_raw = str(row['Class']).strip() if 'Class' in df.columns and pd.notna(row['Class']) else ''
                    bias_raw = str(row['Stat_Bias']).strip() if 'Stat_Bias' in df.columns and pd.notna(row['Stat_Bias']) else ''

                    rare = character_data[char_id].rare if (not rare_raw or rare_raw.startswith('=')) else rare_raw
                    role = character_data[char_id].type if (not role_raw or role_raw.startswith('=')) else role_raw
                    cls = str(row['Class']).strip() if ('Class' in df.columns and pd.notna(row['Class']) and not str(row['Class']).startswith('=')) else ('Boss' if 'Boss' in char_id else ('Creep' if char_id in ['VanguardTiger', 'RatMonster', 'CrabSolider', 'SquidSolider', 'Benborba', 'Baborben', 'YoungBufflalo', 'Minions', 'HeavenlySoddier'] else 'Character'))
                    bias = 'Balanced' if (not bias_raw or bias_raw.startswith('=')) else bias_raw

                    r_mult, r_tier = rarity_mult.get(rare, (1.0, 0)), rarity_tier.get(rare, 0)
                    c_hp_mult, c_atk_mult, c_def_mult, c_res_bonus = class_dict.get(cls, (1.0, 1.0, 1.0, 0))
                    b_hp, b_atk, b_def, b_spd, b_cr, b_cd, b_shred, b_pen, b_res = bias_dict.get(bias, bias_dict['Balanced'])

                    base_st = role_base_data.get(role, role_base_data['Fighter'])
                    inc_st = role_inc_data.get(role, role_inc_data['Fighter'])

                    calc_hp = round(base_st['hp'] * r_mult * c_hp_mult * b_hp)
                    calc_atk = round(base_st['atk'] * r_mult * c_atk_mult * b_atk)
                    calc_def = round(base_st['def'] * r_mult * c_def_mult * b_def)
                    calc_spd = base_st['speed'] + (r_tier * inc_st['speed']) + b_spd
                    calc_shred = base_st['def_shred'] + (r_tier * inc_st['def_shred']) + b_shred
                    calc_cr = base_st['crit_rate'] + (r_tier * inc_st['crit_rate']) + b_cr
                    calc_cd = base_st['crit_dmg'] + (r_tier * inc_st['crit_dmg']) + b_cd
                    calc_pen = base_st['penetration'] + (r_tier * inc_st['penetration']) + b_pen
                    calc_res = base_st['crit_dmg_res'] + (r_tier * inc_st['crit_dmg_res']) + c_res_bonus + b_res

                    char_stats = {}
                    char_stats['hp'] = parse_val(row.get('hp')) or calc_hp
                    char_stats['atk'] = parse_val(row.get('atk')) or calc_atk
                    char_stats['def'] = parse_val(row.get('def')) or calc_def
                    char_stats['speed'] = parse_val(row.get('speed')) or calc_spd
                    char_stats['def_shred'] = parse_val(row.get('def_shred')) or calc_shred
                    char_stats['crit_rate'] = parse_val(row.get('crit_rare')) or parse_val(row.get('crit_rate')) or calc_cr
                    char_stats['crit_dmg'] = parse_val(row.get('crit_dmg')) or calc_cd
                    char_stats['penetration'] = parse_val(row.get('penetration')) or calc_pen
                    char_stats['crit_dmg_res'] = parse_val(row.get('crit_dmg_res')) or calc_res

                    character_data[char_id].stats = char_stats

        if "CharacterAttribute" in all_sheets:
            df = all_sheets["CharacterAttribute"]

            for _, row in df.iterrows():
                char_id = str(row['ID']).strip()
                if char_id in character_data:
                    attr_type = str(row['AttributeType']).strip()

                    attr_comp = AttributeComponent(
                        max_stat_type = str(row['StatLink']).strip() if pd.notna(row['StatLink']) else "None",
                        start_percent = float(row['StartPercent']) if pd.notna(row['StartPercent']) else 0.0
                    )
                    character_data[char_id].attributes[attr_type] = attr_comp

        if "CharacterUpgrade" in all_sheets:
            df = all_sheets["CharacterUpgrade"]
            
            for _, row in df.iterrows():
                char_id = str(row['ID']).strip()
                if char_id in character_data:
                    base_hp = character_data[char_id].stats.get('hp', 0)
                    base_atk = character_data[char_id].stats.get('atk', 0)
                    base_def = character_data[char_id].stats.get('def', 0)

                    upg_stats = {}
                    upg_stats['hp'] = parse_val(row.get('hp')) or round(base_hp * 0.05)
                    upg_stats['atk'] = parse_val(row.get('atk')) or round(base_atk * 0.05)
                    upg_stats['def'] = parse_val(row.get('def')) or round(base_def * 0.05)
                    character_data[char_id].upgrades = upg_stats
        
        final_data = {character_id: character.to_dict() for character_id, character in character_data.items()}
        self.export_json(config.OUTPUT_GAME_CONFIG_FOLDER, final_data, "CharacterConfig")
