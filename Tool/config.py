from pathlib import Path

ROOT_DIR = Path(__file__).resolve().parent

INPUT_FOLDER = ROOT_DIR / "data/"
OUTPUT_FOLDER = ROOT_DIR /"../Assets/Data_V2/GameConfig/"

LOCALIZATION_FILE = "Localizations.xlsx"
GAME_CONFIG_FILE = "Config.xlsx"
LOCALIZATION = ROOT_DIR/"../Assets/Scripts/Core/Localization"

OUTPUT_FOLDER.mkdir(parents=True, exist_ok=True)