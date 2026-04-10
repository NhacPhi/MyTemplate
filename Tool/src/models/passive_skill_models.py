import json
from dataclasses import dataclass, asdict
from typing import Optional, List, Union

@dataclass
class PassiveSkillModel:
    desc_template_hash: int
    param0_values: Union[List[float], str]
    param1_values: Union[List[float], str]

    def __post_init__(self):
        if isinstance(self.param0_values, str):
            self.param0_values = self._parse_array(self.param0_values)
            
        if isinstance(self.param1_values, str):
            self.param1_values = self._parse_array(self.param1_values)

    def _parse_array(self, value_str: str) -> List[float]:
        if not value_str or str(value_str).strip() == "":
            return []
        
        return [float(x.strip()) for x in str(value_str).split(',') if x.strip()]
    
    
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}