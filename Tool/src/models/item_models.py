from dataclasses import dataclass, field, asdict
from typing import List, Optional, Dict

@dataclass
class WeaponComponent:
    weapon_type: str
    passive_id: str
    stats: Dict[str, int] = field(default_factory=dict)
    upgrades: Dict[str, int] = field(default_factory=dict)

@dataclass
class ExpComponent:
    value: int

@dataclass
class MainStatData:
    type: str
    value: float
    mod_type: str

@dataclass
class ArmorComponent:
    part: str
    armor_set: str
    substat_pool_id: str
    main_stat: Optional[MainStatData] = None


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
