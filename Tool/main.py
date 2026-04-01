from src import Generate_Hash_String
from src.build_configs import ItemConfigBuilder, CharacterConfigBuilder
from src.build_character_config import CharacterUpdateLeveConfig
from src.build_narrative import ActorConfigBuilder, DialogueConfigBuilder, QuestLineConfigBuilder
import config
import os



def main():
    loc_path = os.path.join(config.INPUT_FOLDER, config.LOCALIZATION_FILE)
    Generate_Hash_String.export_localization(loc_path)


    config_path = os.path.join(config.INPUT_FOLDER, config.GAME_CONFIG_FILE)
    item_config = ItemConfigBuilder(config_path)
    item_config.run()
    level_config = CharacterUpdateLeveConfig(config_path)
    level_config.run()

    character_config = CharacterConfigBuilder(config_path)
    character_config.run()

    narrative_path = os.path.join(config.INPUT_FOLDER, config.GAME_NARRATIVE_CONFIG_FILE)
    ator_config = ActorConfigBuilder(narrative_path)
    ator_config.run()

    dialogue_config = DialogueConfigBuilder(narrative_path)
    dialogue_config.run()

    questline_config = QuestLineConfigBuilder(narrative_path)
    questline_config.run()
if __name__  == "__main__":
    main()