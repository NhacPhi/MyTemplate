from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parent

INPUT_FOLDER = ROOT_DIR / "data/"

OUTPUT_GAME_LOCALIZATION_FOLDER = ROOT_DIR /"../Assets/Data/Localization/"
OUTPUT_GAME_CONFIG_FOLDER = ROOT_DIR /"../Assets/Data/GameConfig/"
OUTPUT_GAME_NARRATIVE_FOLDER = ROOT_DIR /"../Assets/Data/Narrative/"

LOCALIZATION_FILE = "Localizations.xlsx"
GAME_CONFIG_FILE = "GameConfig.xlsx"
GAME_NARRATIVE_CONFIG_FILE = "GameNarrative.xlsx"

LOCALIZATION = ROOT_DIR/"../Assets/Scripts/Core/Localization"
OUTPUT_GAME_CONFIG_FOLDER.mkdir(parents=True, exist_ok=True)