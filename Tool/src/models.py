from dataclasses import dataclass, field, asdict
from typing import List, Optional, Dict, Any

@dataclass
class WeaponComponent:
    weapon_type: str
    stats: Dict[str, int] = field(default_factory=dict)
    upgrades: Dict[str, int] = field(default_factory=dict)

@dataclass
class ExpComponent:
    value: int

@dataclass
class ArmorComponent:
    part: str
    armor_set: str

@dataclass
class ItemModel:
    name_hash: int
    description_hash: int
    use_hash: int
    item_type: str
    rarity: str

    weapon_data: Optional[WeaponComponent] = None
    exp_data: Optional[ExpComponent] = None
    armor_data: Optional[ArmorComponent] = None

    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}

@dataclass
class CharacterModel:
    name_hash: int
    rare: str
    type: str
    stats: Dict[str, int] = field(default_factory=dict) # stats config
    upgrades: Dict[str, int] = field(default_factory=dict) # stats upgrade

    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}