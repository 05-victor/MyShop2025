using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Foundation;

namespace MyShop.Client.Views.Components.Forms;

/// <summary>
/// A reusable date range picker with start and end dates.
/// </summary>
public sealed partial class DateRangePicker : UserControl
{
    public DateRangePicker()
    {
        this.InitializeComponent();
    }

    #region StartLabel Property

    public static readonly DependencyProperty StartLabelProperty =
        DependencyProperty.Register(
            nameof(StartLabel),
            typeof(string),
            typeof(DateRangePicker),
            new PropertyMetadata("Start Date"));

    public string StartLabel
    {
        get => (string)GetValue(StartLabelProperty);
        set => SetValue(StartLabelProperty, value);
    }

    #endregion

    #region EndLabel Property

    public static readonly DependencyProperty EndLabelProperty =
        DependencyProperty.Register(
            nameof(EndLabel),
            typeof(string),
            typeof(DateRangePicker),
            new PropertyMetadata("End Date"));

    public string EndLabel
    {
        get => (string)GetValue(EndLabelProperty);
        set => SetValue(EndLabelProperty, value);
    }

    #endregion

    #region StartPlaceholder Property

    public static readonly DependencyProperty StartPlaceholderProperty =
        DependencyProperty.Register(
            nameof(StartPlaceholder),
            typeof(string),
            typeof(DateRangePicker),
            new PropertyMetadata("Select start date"));

    public string StartPlaceholder
    {
        get => (string)GetValue(StartPlaceholderProperty);
        set => SetValue(StartPlaceholderProperty, value);
    }

    #endregion

    #region EndPlaceholder Property

    public static readonly DependencyProperty EndPlaceholderProperty =
        DependencyProperty.Register(
            nameof(EndPlaceholder),
            typeof(string),
            typeof(DateRangePicker),
            new PropertyMetadata("Select end date"));

    public string EndPlaceholder
    {
        get => (string)GetValue(EndPlaceholderProperty);
        set => SetValue(EndPlaceholderProperty, value);
    }

    #endregion

    #region StartDate Property

    public static readonly DependencyProperty StartDateProperty =
        DependencyProperty.Register(
            nameof(StartDate),
            typeof(DateTimeOffset?),
            typeof(DateRangePicker),
            new PropertyMetadata(null));

    public DateTimeOffset? StartDate
    {
        get => (DateTimeOffset?)GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    #endregion

    #region EndDate Property

    public static readonly DependencyProperty EndDateProperty =
        DependencyProperty.Register(
            nameof(EndDate),
            typeof(DateTimeOffset?),
            typeof(DateRangePicker),
            new PropertyMetadata(null));

    public DateTimeOffset? EndDate
    {
        get => (DateTimeOffset?)GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }

    #endregion

    #region Events

    public event TypedEventHandler<CalendarDatePicker, CalendarDatePickerDateChangedEventArgs> StartDateChanged;
    public event TypedEventHandler<CalendarDatePicker, CalendarDatePickerDateChangedEventArgs> EndDateChanged;

    private void StartDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        StartDate = args.NewDate;
        StartDateChanged?.Invoke(sender, args);
    }

    private void EndDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
    {
        EndDate = args.NewDate;
        EndDateChanged?.Invoke(sender, args);
    }

    #endregion
}
