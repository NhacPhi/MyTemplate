from dataclasses import dataclass, field, asdict
from typing import List, Optional, Dict

@dataclass
class CostItem:
    id:str
    quantity:int

@dataclass
class TierData:
    unlock_max_level:int
    level_req: int
    max:int
    cost: List[CostItem] = field(default_factory=list)

@dataclass
class AscensionTemplate:
    max_tier: int = 0
    tier: Dict[str, TierData] = field(default_factory=dict)
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}
    
@dataclass
class ExpCurveConfig:
    total_exp: Dict[str, int] = field(default_factory=dict)
    up_exp: Dict[str, int] = field(default_factory=dict)

    def to_dict(self):
        return asdict(self)

@dataclass
class AttributeComponent:
    max_stat_type: str
    start_percent: float

@dataclass
class SkillComponent:
    id: str
    name_hash: int
    des_hash: int
    skill: str
    skill_type: str
    target_type: str
    damage_multiplier: List[float]
    max_cooldown: List[int]
    flat_damage: float
    effect_id: str
    sound: str = ""
    passive_id: str = ""

@dataclass
class CharacterModel:
    name_hash: int
    rare: str
    type: str
    skills: Dict[str, SkillComponent]= field(default_factory=dict) # skill config
    stats: Dict[str, int] = field(default_factory=dict) # stats config
    attributes: Dict[str, AttributeComponent] = field(default_factory=dict) # attribute config
    upgrades: Dict[str, int] = field(default_factory=dict) # stats upgrade

    def __post_init__(self):
        # auto add shield 
        if "shield" not in self.attributes:
            self.attributes["shield"] = AttributeComponent(max_stat_type="None", start_percent=0.0)

    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}
