using MyShop.Core.Common;
using MyShop.Core.Interfaces.Services;
using System.Text;

namespace MyShop.Client.Services;

/// <summary>
/// Service for exporting data to CSV/Excel formats
/// </summary>
public class ExportService : IExportService
{
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
            var tempPath = Path.Combine(Path.GetTempPath(), fullFileName);

            // Write to file
            await File.WriteAllTextAsync(tempPath, csv.ToString());

            System.Diagnostics.Debug.WriteLine($"[ExportService] Exported {data.Count()} rows to {fullFileName}");
            return Result<string>.Success(tempPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ExportService] ExportToCsvAsync failed: {ex.Message}");
            return Result<string>.Failure($"Failed to export CSV: {ex.Message}", ex);
        }
    }
}
