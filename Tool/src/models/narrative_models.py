from dataclasses import dataclass, field, asdict
from typing import List, Optional, Dict

@dataclass
class DialogueChoiceModel:
    text_hash: int
    action_type: int
    target_node_id: str = ""
    param: str = ""

@dataclass
class DialogueNodeModel:
    node_id: str
    actor_id: str
    text_hash: int
    next_node_id: str = ""
    step_end: bool = False
    choices: Optional[List[DialogueChoiceModel]] = None

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
    id: str = ""
    type: str = ""
    nodes: Optional[List[DialogueNodeModel]] = None
    lines: Optional[List[LineModel]] = None
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}

@dataclass
class ActorMode:
    name_hash: int
    dialogue_default: str
    location_hash: int
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}

@dataclass
class StepModel:
    id: str
    actor_id: str
    previous_dialogue: str
    completed_dialogue: str
    incomplete_dialogue: str
    type: str
    item_id: str
    des_hash: int = 0
    target_id: str = ""
    required_amount: int = 1

@dataclass
class QuestModel:
    id: str
    name_hash: int
    des_hash: int
    steps: List[StepModel]
    chapter_id: str = ""
    prerequisite_quest_ids: List[str] = field(default_factory=list)
    required_level: int = 1
    quest_type: int = 0
    reward_id: str = ""

@dataclass
class QuestLinesModel:
    id: str
    name_hash: int
    quests: List[QuestModel]
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}

@dataclass
class DailyQuestModel:
    id: str
    name_hash: int
    des_hash: int
    target_hash: int
    location_hash: int
    reward_id: str
    objective_type: str
    target_id: str
    require_amount: int
    def to_dict(self):
        return {k: v for k, v in asdict(self).items() if v is not None}
