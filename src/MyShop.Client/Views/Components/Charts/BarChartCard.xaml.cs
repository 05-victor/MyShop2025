using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.DataTransfer;

namespace MyShop.Client.Views.Components.Charts
{
    public sealed partial class BarChartCard : UserControl
    {
        public BarChartCard()
        {
            this.InitializeComponent();
        }

        #region Events

        /// <summary>
        /// Event raised when user requests to refresh data
        /// </summary>
        public event EventHandler? RefreshRequested;

        /// <summary>
        /// Event raised when user requests to export data
        /// </summary>
        public event EventHandler<string>? ExportRequested;

        #endregion

        #region Menu Event Handlers

        private void RefreshMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RefreshRequested?.Invoke(this, EventArgs.Empty);
        }

        private void CopyDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var csvData = GetSeriesAsCsv();
                var dataPackage = new DataPackage();
                dataPackage.SetText(csvData);
                Clipboard.SetContent(dataPackage);
                
                Services.LoggingService.Instance.Debug($"[{Title}] Data copied to clipboard");
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error($"[{Title}] Failed to copy data", ex);
            }
        }

        private void ExportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var csvData = GetSeriesAsCsv();
            ExportRequested?.Invoke(this, csvData);
        }

        private async void ExportPngMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var exportService = App.Current.Services.GetService(typeof(Services.IChartExportService)) as Services.IChartExportService;
                if (exportService != null)
                {
                    await exportService.ExportChartAsPngAsync(ChartControl, Title);
                }
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error($"[{Title}] Failed to export PNG", ex);
            }
        }

        private async void ExportPdfMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var exportService = App.Current.Services.GetService(typeof(Services.IChartExportService)) as Services.IChartExportService;
                if (exportService != null)
                {
                    await exportService.ExportChartAsPdfAsync(ChartControl, Title, Subtitle);
                }
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error($"[{Title}] Failed to export PDF", ex);
            }
        }

        private async void ExportCsvMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var exportService = App.Current.Services.GetService(typeof(Services.IChartExportService)) as Services.IChartExportService;
                if (exportService != null)
                {
                    await exportService.ExportChartDataAsCsvAsync(Series, Title);
                }
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error($"[{Title}] Failed to export CSV", ex);
            }
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise event or call command to retry loading
            RefreshMenuItem_Click(sender, e);
        }

        private string GetSeriesAsCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Chart: {Title}");
            sb.AppendLine("Category,Value");

            if (Series != null)
            {
                int seriesIndex = 0;
                foreach (var series in Series)
                {
                    sb.AppendLine($"Series {seriesIndex + 1}: {series.Name ?? "Unnamed"}");
                    if (series.Values != null)
                    {
                        int index = 0;
                        foreach (var value in series.Values)
                        {
                            sb.AppendLine($"{index},{value}");
                            index++;
                        }
                    }
                    seriesIndex++;
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(BarChartCard), new PropertyMetadata("Bar Chart"));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(BarChartCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ShowSubtitleProperty =
            DependencyProperty.Register(nameof(ShowSubtitle), typeof(Visibility), typeof(BarChartCard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ShowMenuProperty =
            DependencyProperty.Register(nameof(ShowMenu), typeof(Visibility), typeof(BarChartCard), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ShowViewToggleProperty =
            DependencyProperty.Register(nameof(ShowViewToggle), typeof(Visibility), typeof(BarChartCard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register(nameof(Series), typeof(IEnumerable<ISeries>), typeof(BarChartCard), new PropertyMetadata(null));

        public static readonly DependencyProperty XAxesProperty =
            DependencyProperty.Register(nameof(XAxes), typeof(IEnumerable<Axis>), typeof(BarChartCard), new PropertyMetadata(null));

        public static readonly DependencyProperty YAxesProperty =
            DependencyProperty.Register(nameof(YAxes), typeof(IEnumerable<Axis>), typeof(BarChartCard), new PropertyMetadata(null));

        public static readonly DependencyProperty ChartHeightProperty =
            DependencyProperty.Register(nameof(ChartHeight), typeof(double), typeof(BarChartCard), new PropertyMetadata(300.0));

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(BarChartCard), new PropertyMetadata(false));

        public static readonly DependencyProperty IsEmptyProperty =
            DependencyProperty.Register(nameof(IsEmpty), typeof(bool), typeof(BarChartCard), new PropertyMetadata(false));

        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(BarChartCard), new PropertyMetadata(false));

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(BarChartCard), new PropertyMetadata(string.Empty));

        #endregion

        #region Properties

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public Visibility ShowSubtitle
        {
            get => (Visibility)GetValue(ShowSubtitleProperty);
            set => SetValue(ShowSubtitleProperty, value);
        }

        public Visibility ShowMenu
        {
            get => (Visibility)GetValue(ShowMenuProperty);
            set => SetValue(ShowMenuProperty, value);
        }

        public Visibility ShowViewToggle
        {
            get => (Visibility)GetValue(ShowViewToggleProperty);
            set => SetValue(ShowViewToggleProperty, value);
        }

        public IEnumerable<ISeries> Series
        {
            get => (IEnumerable<ISeries>)GetValue(SeriesProperty);
            set => SetValue(SeriesProperty, value);
        }

        public IEnumerable<Axis> XAxes
        {
            get => (IEnumerable<Axis>)GetValue(XAxesProperty);
            set => SetValue(XAxesProperty, value);
        }

        public IEnumerable<Axis> YAxes
        {
            get => (IEnumerable<Axis>)GetValue(YAxesProperty);
            set => SetValue(YAxesProperty, value);
        }

        public double ChartHeight
        {
            get => (double)GetValue(ChartHeightProperty);
            set => SetValue(ChartHeightProperty, value);
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public bool IsEmpty
        {
            get => (bool)GetValue(IsEmptyProperty);
            set => SetValue(IsEmptyProperty, value);
        }

        public bool HasError
        {
            get => (bool)GetValue(HasErrorProperty);
            set => SetValue(HasErrorProperty, value);
        }

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        #endregion
    }
}
