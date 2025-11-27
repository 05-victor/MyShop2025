using LiveChartsCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyShop.Client.Views.Components.Charts
{
    public class LegendItem
    {
        public string Color { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public sealed partial class PieChartCard : UserControl
    {
        public PieChartCard()
        {
            this.InitializeComponent();
        }

        #region Dependency Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(PieChartCard), new PropertyMetadata("Distribution"));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(PieChartCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ShowSubtitleProperty =
            DependencyProperty.Register(nameof(ShowSubtitle), typeof(Visibility), typeof(PieChartCard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ShowMenuProperty =
            DependencyProperty.Register(nameof(ShowMenu), typeof(Visibility), typeof(PieChartCard), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ShowViewToggleProperty =
            DependencyProperty.Register(nameof(ShowViewToggle), typeof(Visibility), typeof(PieChartCard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ShowLegendProperty =
            DependencyProperty.Register(nameof(ShowLegend), typeof(Visibility), typeof(PieChartCard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register(nameof(Series), typeof(IEnumerable<ISeries>), typeof(PieChartCard), new PropertyMetadata(null));

        public static readonly DependencyProperty LegendItemsProperty =
            DependencyProperty.Register(nameof(LegendItems), typeof(ObservableCollection<LegendItem>), typeof(PieChartCard), new PropertyMetadata(null));

        public static readonly DependencyProperty ChartHeightProperty =
            DependencyProperty.Register(nameof(ChartHeight), typeof(double), typeof(PieChartCard), new PropertyMetadata(300.0));

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

        public Visibility ShowLegend
        {
            get => (Visibility)GetValue(ShowLegendProperty);
            set => SetValue(ShowLegendProperty, value);
        }

        public IEnumerable<ISeries> Series
        {
            get => (IEnumerable<ISeries>)GetValue(SeriesProperty);
            set => SetValue(SeriesProperty, value);
        }

        public ObservableCollection<LegendItem> LegendItems
        {
            get => (ObservableCollection<LegendItem>)GetValue(LegendItemsProperty);
            set => SetValue(LegendItemsProperty, value);
        }

        public double ChartHeight
        {
            get => (double)GetValue(ChartHeightProperty);
            set => SetValue(ChartHeightProperty, value);
        }

        #endregion
    }
}
