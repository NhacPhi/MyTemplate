using UnityEngine;
using UnityEditor;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text;
using System.Collections.Generic;

public static class CsvToTextConverter
{
    [MenuItem("Localization/Convert CSV to TXT")]
    public static void Convert()
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
}

