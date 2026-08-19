from dataclasses import dataclass, asdict
from typing import Optional, List, Dict, Any

@dataclass
class SetBonusModel:
    name_hash: int
    pieces: int 
    stat: str
    value: float
    modifier_type: str
    stats: Optional[List[Dict[str, Any]]] = None

    def to_dict(self):
        d = {
            'name_hash': self.name_hash,
            'pieces': self.pieces,
            'stat': self.stat,
            'value': self.value,
            'modifier_type': self.modifier_type
        }
        if self.stats:
            d['stats'] = self.stats
        return d