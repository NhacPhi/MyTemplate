using UnityEngine;
using UnityEditor;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text;
using System.Collections.Generic;
using Tech.Json;
using System;
using System.Reflection;
using NPOI.Util.Collections;
using System.Linq;
using NUnit.Framework;

public static class CsvToTextConverter
{
    [MenuItem("ConvertCSV/Localization")]
    public static void ConvertLocalization()
    {
        string excelPath = "Assets/Data/Localizations.xlsx";

        if (!File.Exists(excelPath))
        {
            Debug.LogError("Don't find the file path: " + excelPath);
            return;
        }

        using FileStream stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read);
        IWorkbook workbook = new XSSFWorkbook(stream);

        Dictionary<string, StringBuilder> languageBuilders = new Dictionary<string, StringBuilder>();

        for (int s = 0; s < workbook.NumberOfSheets; s++)
        {
            ISheet sheet = workbook.GetSheetAt(s);
            if (sheet == null)
            {
                Debug.LogWarning($"Sheet {s} empty or error, skip.");
                continue;
            }

            string sheetName = sheet.SheetName;
            Debug.Log($" Loading sheet: {sheetName}");

            // Dòng đầu là header (Key, EN, VI, JP, ...)
            IRow headerRow = sheet.GetRow(0);
            if (headerRow == null)
            {
                Debug.LogWarning($" Sheet '{sheetName}' hasn't the header, skip.");
                continue;
            }

            int colCount = headerRow.LastCellNum;

            // Lặp qua từng cột ngôn ngữ
            for (int col = 1; col < colCount; col++) // bỏ cột Key
            {
                string lang = headerRow.GetCell(col)?.ToString()?.Trim();
                if (string.IsNullOrEmpty(lang))
                    continue;

                if (!languageBuilders.ContainsKey(lang))
                    languageBuilders[lang] = new StringBuilder();

                // Lặp qua từng dòng (Key + Value)
                for (int row = 1; row <= sheet.LastRowNum; row++)
                {
                    IRow dataRow = sheet.GetRow(row);
                    if (dataRow == null) continue;

                    string key = dataRow.GetCell(0)?.ToString()?.Trim();
                    string value = dataRow.GetCell(col)?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(key))
                        continue;

                    // Ghi "Key=Value"
                    languageBuilders[lang].AppendLine($"{key}={value}");
                }
            }

            // Xuất từng ngôn ngữ ra file .txt
            foreach (var kvp in languageBuilders)
            {
                string lang = kvp.Key;
                string outPath = $"Assets/Data/Localization/Localization_{lang}.txt";

                File.WriteAllText(outPath, kvp.Value.ToString(), Encoding.UTF8);
                Debug.Log($" Exported {lang} → {outPath}");
            }

            AssetDatabase.Refresh();
            Debug.Log("Convert've done for all sheet!");
        }
    }

    [MenuItem("ConvertCSV/GameConfig")]
    public static void ConvertGameItem()
    {
        string excelPath = "Assets/Data/GameConfig.xlsx";

        if (!File.Exists(excelPath))
        {
            Debug.LogError("Don't find the file path: " + excelPath);
            return;
        }

        using (FileStream stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new XSSFWorkbook(stream);

            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                ISheet sheet = workbook.GetSheetAt(i);
                string sheetName = sheet.SheetName;
                Debug.Log($"Đang xử lý sheet: {sheetName}");

                switch (sheetName)
                {
                    case "Food":
                        ExportGameConfigSheet<FoodConfig>(sheet, sheetName);
                        break;
                    case "Weapon":
                        ExportGameConfigSheet<WeaponConfig>(sheet, sheetName);
                        break;
                    case "GemStone":
                        ExportGameConfigSheet<GemStoneConfig>(sheet, sheetName);
                        break;
                    case "BaseArmor":
                        ExportGameConfigSheet<BaseArmorConfig>(sheet, sheetName);
                        break;
                    case "Shard":
                        ExportGameConfigSheet<ShardConfig>(sheet, sheetName);
                        break;
                    case "Character":
                        ExportGameConfigSheet<CharacterConfig>(sheet, sheetName);
                        break;
                    case "CharacterStat":
                        ExportGameConfigSheet<CharacterStatConfig>(sheet, sheetName);
                        break;
                    case "CharacterUpgrade":
                        ExportGameConfigSheet<CharacterUpgradeConfig>(sheet, sheetName);
                        break;
                    case "Exp":
                        ExportGameConfigSheet<ExpConfig>(sheet, sheetName);
                        break;
                    default: break;
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Convert've done for all sheet!");
    }

    [MenuItem("ConvertCSV/Narrative")]
    public static void ConvertNarrative()
    {
        string excelPath = "Assets/Data/GameNarrative.xlsx";

        if (!File.Exists(excelPath))
        {
            Debug.LogError("Don't find the file path: " + excelPath);
            return;
        }

        using (FileStream stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new XSSFWorkbook(stream);

            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                ISheet sheet = workbook.GetSheetAt(i);
                string sheetName = sheet.SheetName;
                Debug.Log($"Đang xử lý sheet: {sheetName}");

                switch (sheetName)
                {
                    case "Actors":
                        ExportNarrativeSheet<ActorConfig>(sheet, sheetName);
                        break;
                    case "Dialogues":
                        ExportNarrativeSheet<DialogueConfig>(sheet, sheetName);
                        break;
                    case "Lines":
                        ExportNarrativeSheet<LineConfig>(sheet, sheetName);
                        break;
                    case "Choices":
                        ExportNarrativeSheet<ChoiceConfig>(sheet, sheetName);
                        break;
                    case "Rewards":
                        ExportRewards(sheetName, excelPath);
                        break;
                    case "QuestLines":
                        ExportNarrativeSheet<QuestLineConfig>(sheet, sheetName);
                        break;
                    case "Quests":
                        ExportNarrativeSheet<QuestConfig>(sheet, sheetName);
                        break;
                    case "Steps":
                        ExportNarrativeSheet<StepConfig>(sheet, sheetName);
                        break;
                    default: break;
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Convert've done for all sheet!");
    }
    private static void ExportGameConfigSheet<T>(ISheet sheet, string sheetName)
    {
        List<T> list = new List<T>();

        // Lấy header
        IRow header = sheet.GetRow(0);
        int colCount = header.LastCellNum;


        bool hasValue;
        // Đọc từng dòng
        for (int r = 1; r <= sheet.LastRowNum; r++)
        {
            hasValue = true;
            IRow row = sheet.GetRow(r);
            if (row == null) continue;

            T obj = System.Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Dictionary<string, PropertyInfo> propMap = properties.ToDictionary(p => p.Name.ToLower(), p => p);
            var columnMap = new Dictionary<int, PropertyInfo>();

            for (int c = 0; c < colCount && c < properties.Length; c++)
            {
                string headerName = Normalize(header.GetCell(c)?.StringCellValue);
                if (string.IsNullOrEmpty(headerName)) continue;

                propMap.TryGetValue(headerName, out var prop);
                if (prop == null) continue;

                var field = prop;
                ICell cell = row.GetCell(c);
                if (cell == null) continue;

                try
                {
                    object value = null;
                    switch (cell.CellType)
                    {
                        case CellType.String:
                            if (Enum.TryParse<Rare>(cell.StringCellValue, true, out var rare))
                            {
                                value = rare;
                            }
                            else if (Enum.TryParse<ArmorPart>(cell.StringCellValue, true, out var part))
                            {
                                value = part;
                            }
                            else if (Enum.TryParse<CharacterRare>(cell.StringCellValue, true, out var characterRare))
                            {
                                value = characterRare;
                            }
                            else if (Enum.TryParse<CharacterType>(cell.StringCellValue, true, out var characterType))
                            {
                                value = characterType;
                            }
                            else
                            { 
                                value = cell.StringCellValue;
                            }
                            break;
                        case CellType.Numeric:
                            if (field.PropertyType == typeof(int))
                                value = (int)cell.NumericCellValue;
                            else if (field.PropertyType == typeof(float))
                                value = (float)cell.NumericCellValue;
                            else
                                value = cell.NumericCellValue.ToString();
                            break;
                        case CellType.Boolean:
                            value = cell.BooleanCellValue;
                            break;
                        default:
                            value = cell.ToString();
                            break;
                    }

                    if (value != null)
                        field.SetValue(obj, System.Convert.ChangeType(value, field.PropertyType));
                    else
                    {
                        hasValue = false;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Lỗi đọc cột {c} hàng {r} trong sheet {sheetName}: {ex.Message}");
                }

            }
            if (hasValue)
            {
                list.Add(obj);
            }
        }

        string folder = "Assets/Data/GameConfig/";
        string filePath = Path.Combine(folder, $"{sheetName}.json");
        Json.SaveJson(list, filePath);
        Debug.Log($"Xuất {sheetName}.json ({list.Count} dòng)");
    }

    public static void ExportRewards(string sheetName,string excelFilePath)
    {
        var rewardsDict = new Dictionary<string, RewardPayLoad>();

        using (var fs = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new XSSFWorkbook(fs);
            ISheet sheet = workbook.GetSheet(sheetName);

            for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                string questId = row.GetCell(0)?.ToString()?.Trim();
                string itemId = row.GetCell(1)?.ToString()?.Trim();
                string countStr = row.GetCell(2)?.ToString()?.Trim();

                if (string.IsNullOrEmpty(questId) || string.IsNullOrEmpty(itemId))
                    continue;

                int count = 1;
                int.TryParse(countStr, out count);

                if (!rewardsDict.TryGetValue(questId, out var reward))
                {
                    reward = new RewardPayLoad(questId);
                    rewardsDict[questId] = reward;
                }

                reward.RewardItems.Add(new ItemReward(itemId, count));
            }
        }

        // Chuyển Dictionary → List
        var rewardsList = rewardsDict.Values.ToList();

        string folder = "Assets/Data/Narrative/";
        Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, $"{sheetName}.json");

        Json.SaveJson(rewardsList, filePath);

        Debug.Log($"Xuất {sheetName}.json ({rewardsList.Count} quest)");
    }
    private static void ExportNarrativeSheet<T>(ISheet sheet, string sheetName)
    {
        List<T> list = new List<T>();

        // Lấy header
        IRow header = sheet.GetRow(0);
        int colCount = header.LastCellNum;


        bool hasValue;
        // Đọc từng dòng
        for (int r = 1; r <= sheet.LastRowNum; r++)
        {
            hasValue = true;
            IRow row = sheet.GetRow(r);
            if (row == null) continue;

            T obj = System.Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Dictionary<string, PropertyInfo> propMap = properties.ToDictionary(p => p.Name.ToLower(), p => p);
            var columnMap = new Dictionary<int, PropertyInfo>();

            for (int c = 0; c < colCount && c < properties.Length; c++)
            {
                string headerName = Normalize(header.GetCell(c)?.StringCellValue);
                if (string.IsNullOrEmpty(headerName)) continue;

                propMap.TryGetValue(headerName, out var prop);
                if (prop == null) continue;

                var field = prop;
                ICell cell = row.GetCell(c);
                if (cell == null) continue;

                try
                {
                    object value = null;
                    switch (cell.CellType)
                    {
                        case CellType.String:
                            {
                                if (Enum.TryParse<DialogueType>(cell.StringCellValue, true, out var dialogueType))
                                {
                                    value = dialogueType;
                                }
                                else if (Enum.TryParse<DialogueType>(cell.StringCellValue, true, out var choiceActionType))
                                {
                                    value = choiceActionType;
                                }
                                else if (Enum.TryParse<StepType>(cell.StringCellValue, true, out var stepType))
                                {
                                    value = stepType;
                                }
                                else
                                {
                                    value = cell.StringCellValue;                                  
                                }
                            }
                            break;
                        case CellType.Numeric:
                            if (field.PropertyType == typeof(int))
                                value = (int)cell.NumericCellValue;
                            else if (field.PropertyType == typeof(float))
                                value = (float)cell.NumericCellValue;
                            else
                                value = cell.NumericCellValue.ToString();
                            break;
                        case CellType.Boolean:
                            value = cell.BooleanCellValue;
                            break;
                        default:
                            value = "";//cell.ToString();
                            break;
                    }

                    if (value != null)
                        field.SetValue(obj, System.Convert.ChangeType(value, field.PropertyType));
                    else
                    {
                        hasValue = false;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Lỗi đọc cột {c} hàng {r} trong sheet {sheetName}: {ex.Message}");
                }

            }
            if (hasValue)
            {
                list.Add(obj);
            }
        }

        string folder = "Assets/Data/Narrative/";
        string filePath = Path.Combine(folder, $"{sheetName}.json");
        Json.SaveJson(list, filePath);
        Debug.Log($"Xuất {sheetName}.json ({list.Count} dòng)");
    }
    private static string Normalize(string name)
    {
        return name?.Trim().Replace(" ", "").ToLower() ?? "";
    }


}

