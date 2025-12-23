using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace MyShop.Client.Services;

/// <summary>
/// Service for importing data from CSV files with validation.
/// </summary>
public class CsvImportService
{
    private const int MaxRowsPerImport = 10000;

    /// <summary>
    /// Opens a file picker and reads CSV content.
    /// </summary>
    public async Task<ImportResult<List<Dictionary<string, string>>>> ImportWithPickerAsync()
    {
        try
        {
            var openPicker = new FileOpenPicker();
            
            // Initialize with window handle
            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(openPicker, hwnd);

            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".csv");

            var file = await openPicker.PickSingleFileAsync();
            if (file == null)
            {
                return ImportResult<List<Dictionary<string, string>>>.Cancelled();
            }

            var content = await FileIO.ReadTextAsync(file);
            return ParseCsv(content);
        }
        catch (Exception ex)
        {
            return ImportResult<List<Dictionary<string, string>>>.Failure($"Failed to open file: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses CSV content into a list of dictionaries.
    /// </summary>
    public ImportResult<List<Dictionary<string, string>>> ParseCsv(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            return ImportResult<List<Dictionary<string, string>>>.Failure("File is empty");
        }

        var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length < 2)
        {
            return ImportResult<List<Dictionary<string, string>>>.Failure("File must have at least a header row and one data row");
        }

        if (lines.Length > MaxRowsPerImport + 1)
        {
            return ImportResult<List<Dictionary<string, string>>>.Failure($"Maximum {MaxRowsPerImport} rows allowed per import");
        }

        var headers = ParseCsvLine(lines[0]);
        var results = new List<Dictionary<string, string>>();
        var errors = new List<ImportError>();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i]);
            
            if (values.Count != headers.Count)
            {
                errors.Add(new ImportError(i + 1, $"Column count mismatch. Expected {headers.Count}, got {values.Count}"));
                continue;
            }

            var row = new Dictionary<string, string>();
            for (int j = 0; j < headers.Count; j++)
            {
                row[headers[j]] = values[j];
            }
            results.Add(row);
        }

        return new ImportResult<List<Dictionary<string, string>>>
        {
            IsSuccess = true,
            Data = results,
            TotalRows = lines.Length - 1,
            SuccessCount = results.Count,
            Errors = errors
        };
    }

    /// <summary>
    /// Validates imported data against required columns.
    /// </summary>
    public List<ImportError> ValidateRequiredColumns(
        List<Dictionary<string, string>> data, 
        string[] requiredColumns)
    {
        var errors = new List<ImportError>();

        for (int i = 0; i < data.Count; i++)
        {
            var row = data[i];
            foreach (var col in requiredColumns)
            {
                if (!row.TryGetValue(col, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    errors.Add(new ImportError(i + 2, $"Missing required value for column '{col}'")); // +2 for 1-based + header
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Gets product import template headers.
    /// </summary>
    public string[] GetProductTemplateHeaders() => new[]
    {
        "ProductName", "Category", "Price", "StockQuantity", "Description", "SKU"
    };

    /// <summary>
    /// Gets user import template headers.
    /// </summary>
    public string[] GetUserTemplateHeaders() => new[]
    {
        "Email", "FullName", "Role", "Phone", "IsActive"
    };

    /// <summary>
    /// Generates a template CSV content.
    /// </summary>
    public string GenerateTemplate(string[] headers, string[][] sampleRows = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers));
        
        if (sampleRows != null)
        {
            foreach (var row in sampleRows)
            {
                sb.AppendLine(string.Join(",", row.Select(EscapeCsvValue)));
            }
        }
        
        return sb.ToString();
    }

    private List<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var inQuotes = false;
        var current = new StringBuilder();

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        
        values.Add(current.ToString().Trim());
        return values;
    }

    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}

/// <summary>
/// Result of an import operation.
/// </summary>
public class ImportResult<T>
{
    public bool IsSuccess { get; set; }
    public bool IsCancelled { get; set; }
    public T? Data { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public List<ImportError> Errors { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public static ImportResult<T> Cancelled() => new() { IsCancelled = true };
    public static ImportResult<T> Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}

/// <summary>
/// Represents an import error at a specific row.
/// </summary>
public class ImportError
{
    public int RowNumber { get; set; }
    public string Message { get; set; }

    public ImportError(int row, string message)
    {
        RowNumber = row;
        Message = message;
    }

    public override string ToString() => $"Row {RowNumber}: {Message}";
}
