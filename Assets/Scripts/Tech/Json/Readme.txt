Feature:
Support primitive(int,, foat, string, etc.) and complex data structures(List, Array, Dictionary. etc.).
Uses AES encryption for secure data storage.
[JsonIgore] atribute allows excluding specific varialbes form serialization.
Supports custom serialization by iheriting form JsonConverter<T> and applying [JsonConverter(typeof(YourCustomData))] to a class.
DataService reads JSON files and caches data for optimzed access.

Auto-dectecs save path depending on the enviroment (Editer vs. Build)

Example: 
	#if UNITY_EDITOR
		public const string SavePath = "Assets/.../FileName.json";
	#else
		public const string SavePath = Application.persistentDataPaht + "/.../FileName.json";

Mã hóa đối xứng AES(Advance Encryption Standard)
Sử dụng System.Security.Cryptography 
Ase (key, iv) ramdom and security
Encrypt: 
	Khởi tạo Object Aes (phuong thức mã hóa)
	Tạo cách thức mã hóa dữ liểu encrypter = Aes.CreateEncryptor(key, iv);
	Sử dụng MemoryStream để thao tác với data dưới dàng byte
	Sử dụng CryptoStream chuyển đổi dữ liệu dang steam
	Sử dụng SteeamWirte để ghi dữ liệu ở dạng text 
	Convert data từ dạng byte(nhị phân về dạng ASCII) (gồm các kí tự chữ Latinh)
Decrypt:
	Convert data từ dạng ASCII ra byte[] (nhị phân về dạng ASCII) (gồm các kí tự chữ Latinh)
	Tạo cách thức giải mã dữ liểu decryptor = Aes.CreateDecryptor(key, iv);
	Sử dụng MemoryStream để thao tác với data dưới dàng byte
	Sử dụng CryptoStream chuyển đổi dữ liệu dang steam
	Sử dụng StreamReader để đọc dữ liệu ở dạng text 

Json file:
	Dependencies: UnitTask(git) and Newtonsoft Json package
DataService:
	Load data from Adressable Assets (Text for Json)
	_dataCache cache lại data sau khi load từ Addresable AssetManager
	LoadDataAsync:  check data có trong cache hay chưa nếu có lấy ra TryGetValue nếu không có load từ AddressableAsset
	LoadWithAddressable. AddresambleManager.Instance.LoadAssetSync<TextAsset>(addressKey); save data _dataCahce remove asset khỏi Addressable