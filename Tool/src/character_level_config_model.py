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
