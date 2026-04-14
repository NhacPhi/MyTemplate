from dataclasses import dataclass, asdict
from typing import Optional

@dataclass
class SetBonusModel:
    stat_type: str
    min: float 
    max: float
    modifier_type: str

    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}