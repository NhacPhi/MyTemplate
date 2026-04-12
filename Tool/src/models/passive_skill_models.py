import json
import inspect
from dataclasses import dataclass, asdict, field
from typing import Optional, List, Union, Dict, Any

def parse_float_list(value_str: Union[str, List[float], float, int]) -> List[float]:
    if isinstance(value_str, list):
        return [float(x) for x in value_str]
    
    if value_str is None or str(value_str).strip() == "" or str(value_str).lower() == 'nan':
        return []
    
    if isinstance(value_str, (float, int)): 
        return [float(value_str)]
        
    return [float(x.strip()) for x in str(value_str).split(',') if x.strip()]

@dataclass
class StaticModifierModel:
    stat_type: str
    modify_type: str
    modify_by_upgrade: Union[List[float], str]

    def __post_init__(self):
        self.modify_by_upgrade = parse_float_list(self.modify_by_upgrade)

@dataclass
class CombatEventModel:
    event_type: str
    effect_id: str
    modify_by_upgrade: Union[List[float], str]
    condition_filter: str = ""
    effect_param: float = 0.0
    internal_cooldown: int = 0

    def __post_init__(self):
        self.modify_by_upgrade = parse_float_list(self.modify_by_upgrade)
        
        if not isinstance(self.condition_filter, str) or str(self.condition_filter).lower() == 'nan':
            self.condition_filter = ""
        if self.effect_param is None or str(self.effect_param).lower() == 'nan':
            self.effect_param = 0.0
        if self.internal_cooldown is None or str(self.internal_cooldown).lower() == 'nan':
            self.internal_cooldown = 0

@dataclass
class PassiveSkillModel:
    id: str
    desc_template_hash: str
    static_modifiers: List[Union[StaticModifierModel, dict]] = field(default_factory=list)
    combat_events: List[Union[CombatEventModel, dict]] = field(default_factory=list)

    def __post_init__(self):
        # Auto-cast an toàn: Chỉ lấy các key hợp lệ để tránh lỗi 'unexpected keyword argument'
        if self.static_modifiers:
            valid_keys = inspect.signature(StaticModifierModel).parameters
            self.static_modifiers = [
                StaticModifierModel(**{k: v for k, v in mod.items() if k in valid_keys}) 
                if isinstance(mod, dict) else mod 
                for mod in self.static_modifiers
            ]
            
        if self.combat_events:
            valid_keys = inspect.signature(CombatEventModel).parameters
            self.combat_events = [
                CombatEventModel(**{k: v for k, v in ev.items() if k in valid_keys}) 
                if isinstance(ev, dict) else ev 
                for ev in self.combat_events
            ]

    def to_dict(self):
        # ĐIỂM SỬA CHỮA: Thêm điều kiện k != 'id' để ẩn ID khỏi Object JSON bên trong
        return {k: v for k, v in asdict(self).items() if v is not None and k != 'id'}