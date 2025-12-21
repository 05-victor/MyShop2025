using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using LiveChartsCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace MyShop.Client.Services;

public interface IChartExportService
{
    Task ExportChartAsPngAsync(FrameworkElement chartElement, string title);
    Task ExportChartAsPdfAsync(FrameworkElement chartElement, string title, string subtitle);
    Task ExportChartDataAsCsvAsync(IEnumerable<ISeries> seriesData, string title);
}

public class ChartExportService : IChartExportService
{
    public ChartExportService()
    {
    }

    public async Task ExportChartAsPngAsync(FrameworkElement chartElement, string title)
    {
        try
        {
            var savePicker = new FileSavePicker();
            var window = App.MainWindow;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("PNG Image", new List<string>() { ".png" });
            savePicker.SuggestedFileName = $"{SanitizeFileName(title)}_Chart_{DateTime.Now:yyyyMMdd_HHmmss}";

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Use RenderTargetBitmap to capture chart
                var rtb = new RenderTargetBitmap();
                await rtb.RenderAsync(chartElement);

                var pixelBuffer = await rtb.GetPixelsAsync();
                using var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    (uint)rtb.PixelWidth,
                    (uint)rtb.PixelHeight,
                    96, 96,
                    pixelBuffer.ToArray());

                await encoder.FlushAsync();

                LoggingService.Instance.Information($"Chart exported as PNG: {file.Path}");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to export chart as PNG", ex);
            throw;
        }
    }

    public async Task ExportChartAsPdfAsync(FrameworkElement chartElement, string title, string subtitle)
    {
        try
        {
            var savePicker = new FileSavePicker();
            var window = App.MainWindow;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("PDF Document", new List<string>() { ".pdf" });
            savePicker.SuggestedFileName = $"{SanitizeFileName(title)}_Chart_{DateTime.Now:yyyyMMdd_HHmmss}";

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Capture chart as image first
                var rtb = new RenderTargetBitmap();
                await rtb.RenderAsync(chartElement);
                var pixelBuffer = await rtb.GetPixelsAsync();

                // Convert to SKBitmap for QuestPDF
                var bitmap = new SKBitmap(rtb.PixelWidth, rtb.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
                var pixelArray = pixelBuffer.ToArray();
                var ptr = bitmap.GetPixels();
                System.Runtime.InteropServices.Marshal.Copy(pixelArray, 0, ptr, pixelArray.Length);

                using var imageStream = new MemoryStream();
                bitmap.Encode(imageStream, SKEncodedImageFormat.Png, 100);
                imageStream.Position = 0;

                // Create PDF with QuestPDF
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);

                        page.Header()
                            .Column(col =>
                            {
                                col.Item().Text(title).FontSize(20).Bold();
                                if (!string.IsNullOrEmpty(subtitle))
                                {
                                    col.Item().Text(subtitle).FontSize(12).FontColor(Colors.Grey.Medium);
                                }
                                col.Item().PaddingBottom(10);
                            });

                        page.Content()
                            .Image(imageStream).FitWidth();

                        page.Footer()
                            .AlignCenter()
                            .Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Medium);
                    });
                });

                using var pdfStream = await file.OpenStreamForWriteAsync();
                document.GeneratePdf(pdfStream);

                LoggingService.Instance.Information($"Chart exported as PDF: {file.Path}");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to export chart as PDF", ex);
            throw;
        }
    }

    public async Task ExportChartDataAsCsvAsync(IEnumerable<ISeries> seriesData, string title)
    {
        try
        {
            var savePicker = new FileSavePicker();
            var window = App.MainWindow;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });
            savePicker.SuggestedFileName = $"{SanitizeFileName(title)}_Data_{DateTime.Now:yyyyMMdd_HHmmss}";

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                var csv = new StringBuilder();
                csv.AppendLine($"Chart: {title}");
                csv.AppendLine($"Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                csv.AppendLine();
                
                // Extract data from LiveCharts series
                if (seriesData != null && seriesData.Any())
                {
                    csv.AppendLine("Series,Index,Value");
                    
                    foreach (var series in seriesData)
                    {
                        var seriesName = series.Name ?? "Unnamed Series";
                        if (series.Values != null)
                        {
                            int index = 0;
                            foreach (var value in series.Values)
                            {
                                csv.AppendLine($"\"{seriesName}\",{index},{value}");
                                index++;
                            }
                        }
                    }
                }
                else
                {
                    csv.AppendLine("No data available");
                }

                await FileIO.WriteTextAsync(file, csv.ToString());

                LoggingService.Instance.Information($"Chart data exported as CSV: {file.Path}");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to export chart data as CSV", ex);
            throw;
        }
    }

    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "Chart";

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName
            .Where(c => !invalidChars.Contains(c))
            .ToArray());

        return string.IsNullOrWhiteSpace(sanitized) ? "Chart" : sanitized;
    }
}
