Here are the professional "patch scripts" with English comments, following industry standards for automation and clarity.

1. Windows Batch Script (setup_env.bat)
This script is for Windows users using Command Prompt or PowerShell.

Đoạn mã

@echo off
:: --- Environment Setup Script ---

:: 1. Create the virtual environment if it doesn't exist
echo [1/4] Creating virtual environment (venv)...
if not exist venv (
    python -m venv venv
) else (
    echo Virtual environment already exists.
)

:: 2. Activate the virtual environment
echo [2/4] Activating virtual environment...
call venv\Scripts\activate

:: 3. Upgrade pip to the latest version
echo [3/4] Upgrading pip...
python -m pip install --upgrade pip

:: 4. Install required libraries
echo [4/4] Installing libraries: pandas, mmh3...
pip install pandas mmh3 openpyxl

echo --------------------------------------------------
echo SETUP COMPLETE!
echo To start working, run: call venv\Scripts\activate
echo --------------------------------------------------
pause