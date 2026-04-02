from dataclasses import dataclass, field, asdict
from typing import List, Optional, Dict

@dataclass
class EffectSkillModel:
    name_hash: int
    des_hash: int
    type: str
    target_stat: str
    modify_type: str
    duration: int
    max_stack: int
    value: int 
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}

@dataclass
class StageEnemiesCompoment:
    slot: int
    enemy_id: str
    enemy_level: int
    boss: bool

@dataclass
class BattleModel:
    name_hash: int
    background: str
    reward: str
    enemies: List[StageEnemiesCompoment] = field(default_factory=list)

    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}
