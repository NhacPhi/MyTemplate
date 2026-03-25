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
# Character Config-------------------------
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
    target_type: str
    damage_multiplier: float
    max_cooldown: int
    flat_damage: float
    effect_id: str
    sound: str


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
    
# Effect Skill Config
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
    
# Battle Config-----------------------
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
    enemies: List[StageEnemiesCompoment]

    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}


# Game Narrative -------------------------------------------------------------------------------------
@dataclass
class ChoiceModel:
    text_hash: int
    type: str
    next_dialogue: str

@dataclass
class LineModel:
    text_hash: int
    actor_id: str
    choices: Optional[List[ChoiceModel]] = None

#-----Dialogue Config
@dataclass
class DialogueModel:
    type: str
    lines: List[LineModel]
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}

#----Actor Config
@dataclass
class ActorMode:
    name_hash: int
    dialogue_default: str
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}


#----Questline Config
@dataclass
class StepModel:
    actor_id: str
    previous_diagoue: str
    completed_dialogue: str
    incomplete_dialogue: str
    type: str
    item_id: str
    has_reward: bool
    reward_id: str

@dataclass
class QuestModel:
    name_hash: int
    des_hash: int
    steps: List[StepModel]

@dataclass
class QuestLinesModel:
    name_hash: int
    des_hash: int
    quests: List[QuestModel]
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}




    