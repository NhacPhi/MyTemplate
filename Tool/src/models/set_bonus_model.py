from dataclasses import dataclass, asdict
from typing import Optional

@dataclass
class SetBonusModel:
    name_hash: int
    pieces: int 
    stat: str
    value: float
    modifier_type: str

    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}