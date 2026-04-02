from dataclasses import dataclass, field, asdict
from typing import List, Optional, Dict

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

@dataclass
class DialogueModel:
    type: str
    lines: List[LineModel]
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}

@dataclass
class ActorMode:
    name_hash: int
    dialogue_default: str
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}

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
