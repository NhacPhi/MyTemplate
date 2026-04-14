from src.builders.localization_builder import LocalizationBuilder
from src.builders.item_builder import ItemConfigBuilder
from src.builders.character_builder import CharacterConfigBuilder
from src.builders.character_level_builder import CharacterUpdateLevelBuilder
from src.builders.battle_builder import EffectConfigBuilder, BattleConfigBuilder
from src.builders.narrative_builder import ActorConfigBuilder, DialogueConfigBuilder, QuestLineConfigBuilder
from src.builders.set_bonus_builder import SetBonusBuilder
from src.builders.starup_builder import StarUpBuilder
from src.builders.passive_skill_builder import PassiveSkillBuilder
from src.builders.substat_builder import SubstatPoolBuilder
import config
import os

def main():
    loc_path = os.path.join(config.INPUT_FOLDER, config.LOCALIZATION_FILE)
    loc_builder = LocalizationBuilder(loc_path)
    loc_builder.run()

    config_path = os.path.join(config.INPUT_FOLDER, config.GAME_CONFIG_FILE)
    
    item_config = ItemConfigBuilder(config_path)
    item_config.run()

    passive_config = PassiveSkillBuilder(config_path)
    passive_config.run()
    
    set_bonus_config  = SetBonusBuilder(config_path)
    set_bonus_config.run()

    level_config = CharacterUpdateLevelBuilder(config_path)
    level_config.run()

    character_config = CharacterConfigBuilder(config_path)
    character_config.run()

    effect_config = EffectConfigBuilder(config_path)
    effect_config.run()

    battle_config = BattleConfigBuilder(config_path)
    battle_config.run()

    starup_config = StarUpBuilder(config_path)
    starup_config.run()

    substat_config = SubstatPoolBuilder(config_path)
    substat_config.run()

    narrative_path = os.path.join(config.INPUT_FOLDER, config.GAME_NARRATIVE_CONFIG_FILE)
    ator_config = ActorConfigBuilder(narrative_path)
    ator_config.run()

    dialogue_config = DialogueConfigBuilder(narrative_path)
    dialogue_config.run()

    questline_config = QuestLineConfigBuilder(narrative_path)
    questline_config.run()

if __name__  == "__main__":
    main()