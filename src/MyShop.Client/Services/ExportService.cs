using MyShop.Core.Common;
using MyShop.Core.Interfaces.Services;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace MyShop.Client.Services;

/// <summary>
/// Service for exporting data to CSV/Excel formats with FileSavePicker
/// </summary>
public class ExportService : IExportService
{
    /// <summary>
    /// Export CSV content with FileSavePicker dialog (preferred method)
    /// </summary>
    public async Task<Result<string>> ExportWithPickerAsync(string suggestedFileName, string csvContent)
    {
        try
        {
            var savePicker = new FileSavePicker();
            
            // Initialize with window handle (required for WinUI 3)
            var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("CSV Files", new List<string> { ".csv" });
            savePicker.SuggestedFileName = suggestedFileName;

            var file = await savePicker.PickSaveFileAsync();
            if (file == null)
            {
                // User cancelled
                return Result<string>.Success(string.Empty); // Empty = cancelled, not error
            }

            await FileIO.WriteTextAsync(file, csvContent);

            LoggingService.Instance.Information($"[ExportService] Exported to: {file.Path}");
            return Result<string>.Success(file.Path);
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("[ExportService] Export with picker failed", ex);
            return Result<string>.Failure($"Export failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Build and export CSV with FileSavePicker dialog using builder pattern
    /// </summary>
    public async Task<Result<string>> ExportWithPickerAsync(string suggestedFileName, Action<CsvBuilder> buildCsv)
    {
        var builder = new CsvBuilder();
        buildCsv(builder);
        return await ExportWithPickerAsync(suggestedFileName, builder.ToString());
    }

    public async Task<Result<string>> ExportToCsvAsync<T>(
        IEnumerable<T> data,
        string fileName,
        Func<T, Dictionary<string, string>> columnSelector)
    {
        try
        {
            if (data == null || !data.Any())
            {
                return Result<string>.Failure("No data to export");
            }

            // Extract data and headers from first item
            var rows = data.Select(columnSelector).ToList();
            if (rows.Count == 0)
            {
                return Result<string>.Failure("No data to export");
            }

            var headers = rows[0].Keys.ToArray();

            return await ExportToCsvAsync(rows, fileName, headers);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ExportService] ExportToCsvAsync<T> failed: {ex.Message}");
            return Result<string>.Failure($"Failed to export data: {ex.Message}", ex);
        }
    }

    public async Task<Result<string>> ExportToCsvAsync(
        IEnumerable<Dictionary<string, string>> data,
        string fileName,
        string[] headers)
    {
        try
        {
            if (data == null || !data.Any())
            {
                return Result<string>.Failure("No data to export");
            }

            var csv = new StringBuilder();

            // Write header row
            csv.AppendLine(string.Join(",", headers));

            // Write data rows
            foreach (var row in data)
            {
                var values = headers.Select(h =>
                {
                    var value = row.TryGetValue(h, out var v) ? v : string.Empty;
                    // Escape quotes and wrap in quotes if contains comma, quote, or newline
                    if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                    {
                        value = $"\"{value.Replace("\"", "\"\"")}\"";
                    }
                    return value;
                });
                csv.AppendLine(string.Join(",", values));
            }

            // Generate unique filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fullFileName = $"{fileName}_{timestamp}.csv";

            // Get export directory - use user-specific if available, otherwise temp
            string exportDirectory;
            if (!string.IsNullOrEmpty(StorageConstants.CurrentUserId))
            {
                exportDirectory = StorageConstants.GetUserExportsDirectory(StorageConstants.CurrentUserId);
                StorageConstants.EnsureDirectoryExists(exportDirectory);
            }
            else
            {
                exportDirectory = Path.GetTempPath();
            }

            var exportPath = Path.Combine(exportDirectory, fullFileName);

            // Write to file
            await File.WriteAllTextAsync(exportPath, csv.ToString());

            System.Diagnostics.Debug.WriteLine($"[ExportService] Exported {data.Count()} rows to {exportPath}");
            return Result<string>.Success(exportPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ExportService] ExportToCsvAsync failed: {ex.Message}");
            return Result<string>.Failure($"Failed to export CSV: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Helper class for building CSV content with fluent API
/// </summary>
public class CsvBuilder
{
    private readonly StringBuilder _sb = new();

    public CsvBuilder AddTitle(string title)
    {
        _sb.AppendLine(title);
        return this;
    }

    public CsvBuilder AddHeader(string header)
    {
        _sb.AppendLine($"=== {header} ===");
        return this;
    }

    public CsvBuilder AddLine(string line)
    {
        _sb.AppendLine(line);
        return this;
    }

    public CsvBuilder AddColumnHeaders(params string[] headers)
    {
        _sb.AppendLine(string.Join(",", headers));
        return this;
    }

    public CsvBuilder AddRow(params object[] values)
    {
        var escaped = new List<string>();
        foreach (var val in values)
        {
            var str = val?.ToString() ?? "";
            // Escape quotes and wrap in quotes if contains comma
            if (str.Contains(',') || str.Contains('"') || str.Contains('\n'))
            {
                str = $"\"{str.Replace("\"", "\"\"")}\"";
            }
            escaped.Add(str);
        }
        _sb.AppendLine(string.Join(",", escaped));
        return this;
    }

    public CsvBuilder AddBlankLine()
    {
        _sb.AppendLine();
        return this;
    }

    public CsvBuilder AddMetadata(string key, object value)
    {
        _sb.AppendLine($"{key},{value}");
        return this;
    }

    public override string ToString() => _sb.ToString();
}
