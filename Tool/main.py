from src import Generate_Hash_String
from src.build_configs import ItemConfigBuilder, CharacterConfigBuilder
import config
import os



def main():
    # full_path = os.path.join(config.INPUT_FOLDER , config.LOCALIZATION_FILE)
    # Generate_Hash_String.export_localization(full_path)


    path = os.path.join(config.INPUT_FOLDER, config.GAME_CONFIG_FILE)
    item_config = ItemConfigBuilder(path)
    item_config.run()
    # character_config = CharacterConfigBuilder(path)
    # character_config.run()

if __name__  == "__main__":
    main()