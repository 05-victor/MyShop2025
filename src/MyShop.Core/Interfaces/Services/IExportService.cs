using MyShop.Core.Common;

namespace MyShop.Core.Interfaces.Services;

/// <summary>
/// Service for exporting data to various formats (CSV, Excel)
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Export data to CSV format
    /// </summary>
    /// <typeparam name="T">The type of data to export</typeparam>
    /// <param name="data">Collection of data to export</param>
    /// <param name="fileName">Base file name (without extension)</param>
    /// <param name="columnSelector">Function to select columns and format values</param>
    /// <returns>Result containing file path on success</returns>
    Task<Result<string>> ExportToCsvAsync<T>(
        IEnumerable<T> data,
        string fileName,
        Func<T, Dictionary<string, string>> columnSelector);

    /// <summary>
    /// Export data to CSV with custom headers
    /// </summary>
    /// <param name="data">Collection of rows (each row is a dictionary of column->value)</param>
    /// <param name="fileName">Base file name (without extension)</param>
    /// <param name="headers">Ordered list of column headers</param>
    /// <returns>Result containing file path on success</returns>
    Task<Result<string>> ExportToCsvAsync(
        IEnumerable<Dictionary<string, string>> data,
        string fileName,
        string[] headers);
}
