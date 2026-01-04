1. Tạo môi trường ảo (venv)
    Windows: python -m venv venv

    macOS/Linux: python3 -m venv venv

2. Kích hoạt (Switch) sang venv
    Windows (CMD)	venv\Scripts\activate
    Windows (PowerShell)	.\venv\Scripts\Activate.ps1 
    macOS / Linux	source venv/bin/activate
3. Setting Visual Studio Code   
    Ctrl + Shift + P => Python: Select Interpreter => CHọn Path => venv/Scripts/python.exe
4. Import lib
    Sử dựng lệnh pip: pip install lib_name
    List Lib: pandas (excel or CSV), nmh3(hash map)