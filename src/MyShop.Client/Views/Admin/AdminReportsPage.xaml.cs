using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Admin;
using static MyShop.Client.ViewModels.Admin.AdminReportsViewModel;

namespace MyShop.Client.Views.Admin;

/// <summary>
/// Reports page for Admin role
/// </summary>
public sealed partial class AdminReportsPage : Page
{
    public AdminReportsViewModel ViewModel { get; }

    public AdminReportsPage()
    {
        // COPILOT-FIX STEP 2: Enhanced constructor with comprehensive error handling
        try
        {
            Services.LoggingService.Instance.Debug("[AdminReportsPage] Constructor start");
            
            // Resolve ViewModel
            ViewModel = App.Current.Services.GetRequiredService<AdminReportsViewModel>();
            Services.LoggingService.Instance.Debug("[AdminReportsPage] ViewModel resolved");
            
            // Subscribe to ViewModel events for dialogs
            ViewModel.ViewProductDetailsRequested += OnViewProductDetailsRequested;
            ViewModel.ViewSalespersonDetailsRequested += OnViewSalespersonDetailsRequested;
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[AdminReportsPage] DI FAILED - ViewModel resolution", ex);
            throw; // Re-throw to surface the actual DI issue
        }

        // STEP 2: Wrap InitializeComponent with detailed logging
        try
        {
            Services.LoggingService.Instance.Debug("[AdminReportsPage] Calling InitializeComponent");
            this.InitializeComponent();
            Services.LoggingService.Instance.Debug("[AdminReportsPage] InitializeComponent SUCCESS");
            
            // CRITICAL FIX: Set DataContext so {Binding} resolves to ViewModel
            this.DataContext = ViewModel;
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[AdminReportsPage] XAML PARSE FAILED", ex);
            Services.LoggingService.Instance.Error($"Exception Type: {ex.GetType().FullName}");
            Services.LoggingService.Instance.Error($"Message: {ex.Message}");
            Services.LoggingService.Instance.Error($"StackTrace: {ex.StackTrace}");
            
            // Create minimal fallback UI
            this.Content = new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"❌ XAML CRASH in AdminReportsPage\n\nType: {ex.GetType().Name}\nMessage: {ex.Message}\n\nLogs: {Services.LoggingService.Instance.GetLogDirectory()}",
                Margin = new Microsoft.UI.Xaml.Thickness(24),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas")
            };
            return;
        }
    }

    // COPILOT-FIX: Initialize ViewModel in OnNavigatedTo to avoid constructor crash
    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        try
        {
            base.OnNavigatedTo(e);
            Services.NavigationLogger.LogNavigatedTo(nameof(AdminReportsPage), e.Parameter);
            
            // Set default date range selection
            if (DateRangeComboBox.SelectedIndex < 0 && DateRangeComboBox.Items.Count > 0)
            {
                DateRangeComboBox.SelectedIndex = 0; // Default to "This Week"
            }
            
            // Initialize ViewModel after page is loaded and UI thread is ready
            _ = ViewModel.InitializeCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[AdminReportsPage] OnNavigatedTo failed", ex);
        }
    }

    #region Filter Event Handlers

    private void DateRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Only update ViewModel property, don't auto-reload
        if (DateRangeComboBox.SelectedItem is DateRangeOption option)
        {
            ViewModel.SelectedDateRange = option;
        }
    }

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Only update ViewModel property, don't auto-reload
        if (CategoryComboBox.SelectedItem is string category)
        {
            ViewModel.SelectedCategory = category;
        }
    }

    private void StartDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        // Only update ViewModel property, don't auto-reload
        ViewModel.StartDate = args.NewDate;
    }

    private void EndDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        // Only update ViewModel property, don't auto-reload
        ViewModel.EndDate = args.NewDate;
    }

    private async void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
    {
        // Apply filters and reload data
        await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
    }

    #endregion

    #region Export Event Handler

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ExportReportCommand.ExecuteAsync(null);
    }

    #endregion

    #region View Details Event Handlers

    private void ViewProductDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProductPerformance product)
        {
            ViewModel.ViewProductDetailsCommand.Execute(product);
        }
    }

    private void ViewSalespersonDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Salesperson salesperson)
        {
            ViewModel.ViewSalespersonDetailsCommand.Execute(salesperson);
        }
    }

    private async void OnViewProductDetailsRequested(object? sender, ProductPerformance product)
    {
        await ShowProductDetailsDialogAsync(product);
    }

    private async void OnViewSalespersonDetailsRequested(object? sender, Salesperson salesperson)
    {
        await ShowSalespersonDetailsDialogAsync(salesperson);
    }

    private async Task ShowProductDetailsDialogAsync(ProductPerformance product)
    {
        try
        {
            ProductDetailName.Text = product.Name;
            ProductDetailCategory.Text = product.Category;
            ProductDetailOrders.Text = product.Sold.ToString("N0");
            ProductDetailRevenue.Text = $"${product.Revenue:N2}";
            ProductDetailRating.Text = product.Rating.ToString("F1");
            ProductDetailStock.Text = product.Stock.ToString("N0");

            ProductDetailsDialog.XamlRoot = this.XamlRoot;
            await ProductDetailsDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error("[AdminReportsPage] ShowProductDetailsDialogAsync failed", ex);
        }
    }

    private async Task ShowSalespersonDetailsDialogAsync(Salesperson salesperson)
    {
        try
        {
            SalespersonDetailName.Text = salesperson.Name;
            SalespersonDetailSales.Text = salesperson.Sales.ToString("N0");
            SalespersonDetailRevenue.Text = $"${salesperson.Revenue:N2}";
            // Commission is typically a percentage of revenue
            var commission = salesperson.Revenue * 0.05m; // 5% commission
            SalespersonDetailCommission.Text = $"${commission:N2}";

            SalespersonDetailsDialog.XamlRoot = this.XamlRoot;
            await SalespersonDetailsDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error("[AdminReportsPage] ShowSalespersonDetailsDialogAsync failed", ex);
        }
    }

    #endregion

    #region Chart Menu Event Handlers

    private async void Chart_RefreshRequested(object? sender, EventArgs e)
    {
        // Refresh data when user clicks Refresh in chart menu
        await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
    }

    private async void Chart_ExportRequested(object? sender, string csvData)
    {
        try
        {
            // Get chart title for filename
            string chartTitle = "chart";
            if (sender is Views.Components.Charts.LineChartCard lineChart)
                chartTitle = lineChart.Title;
            else if (sender is Views.Components.Charts.BarChartCard barChart)
                chartTitle = barChart.Title;
            else if (sender is Views.Components.Charts.PieChartCard pieChart)
                chartTitle = pieChart.Title;

            // Clean filename
            chartTitle = chartTitle.Replace(" ", "_").Replace("/", "_");

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            
            // Get the window handle
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, windowHandle);

            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });
            savePicker.SuggestedFileName = $"{chartTitle}_export_{DateTime.Now:yyyyMMdd}";

            var file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                await Windows.Storage.FileIO.WriteTextAsync(file, csvData);
                Services.LoggingService.Instance.Debug($"[AdminReportsPage] Chart data exported to {file.Path}");
            }
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error("[AdminReportsPage] Chart_ExportRequested failed", ex);
        }
    }

    #endregion
}
