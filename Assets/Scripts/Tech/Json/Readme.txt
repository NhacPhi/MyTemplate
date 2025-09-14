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