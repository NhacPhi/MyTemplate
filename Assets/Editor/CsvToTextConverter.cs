using UnityEngine;
using UnityEditor;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text;
using System.Collections.Generic;
using Tech.Json;
using System;

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
                    case "AvatarConfig":
                        ExportSheet<AvatarConfig>(sheet, sheetName);
                        break;
                    case "Weapon":
                        ExportSheet<WeaponConfig>(sheet, sheetName);
                        break;
                    default: break;
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Convert've done for all sheet!");
    }

    private static void ExportSheet<T>(ISheet sheet, string sheetName)
    {
        List<T> list = new List<T>();

        // Lấy header
        IRow header = sheet.GetRow(0);
        int colCount = header.LastCellNum;

        // Đọc từng dòng
        for (int r = 1; r <= sheet.LastRowNum; r++)
        {
            IRow row = sheet.GetRow(r);
            if (row == null) continue;

            T obj = System.Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();

            for (int c = 0; c < colCount && c < properties.Length; c++)
            {
                var field = properties[c];
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
                            else if (Enum.TryParse<WeaponType>(cell.StringCellValue, true, out var type))
                            {
                                value = type;
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
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Lỗi đọc cột {c} hàng {r} trong sheet {sheetName}: {ex.Message}");
                }
                
            }
            list.Add(obj);
        }

        string folder = "Assets/Data/GameConfig/";
        string filePath = Path.Combine(folder, $"{sheetName}.json");
        Json.SaveJson(list, filePath);
        Debug.Log($"Xuất {sheetName}.json ({list.Count} dòng)");
    }
}

