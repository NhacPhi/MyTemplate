@echo off
rem Build Shop data by running create_shop.py
cd /d "%~dp0"
python create_shop.py
if %errorlevel% neq 0 (
    echo [build_shop] Script failed with exit code %errorlevel%
) else (
    echo [build_shop] Success
)
pause
