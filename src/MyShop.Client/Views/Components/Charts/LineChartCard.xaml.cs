using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;

namespace MyShop.Client.Views.Components.Charts
{
    public sealed partial class LineChartCard : UserControl
    {
        public LineChartCard()
        {
            this.InitializeComponent();
        }

        #region Dependency Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(LineChartCard), new PropertyMetadata("Chart Title"));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(LineChartCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ShowSubtitleProperty =
            DependencyProperty.Register(nameof(ShowSubtitle), typeof(Visibility), typeof(LineChartCard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ShowMenuProperty =
            DependencyProperty.Register(nameof(ShowMenu), typeof(Visibility), typeof(LineChartCard), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ShowLegendProperty =
            DependencyProperty.Register(nameof(ShowLegend), typeof(Visibility), typeof(LineChartCard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ShowLegend2Property =
            DependencyProperty.Register(nameof(ShowLegend2), typeof(Visibility), typeof(LineChartCard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty Legend1TextProperty =
            DependencyProperty.Register(nameof(Legend1Text), typeof(string), typeof(LineChartCard), new PropertyMetadata("Series 1"));

        public static readonly DependencyProperty Legend2TextProperty =
            DependencyProperty.Register(nameof(Legend2Text), typeof(string), typeof(LineChartCard), new PropertyMetadata("Series 2"));

        public static readonly DependencyProperty SeriesProperty =
            DependencyProperty.Register(nameof(Series), typeof(IEnumerable<ISeries>), typeof(LineChartCard), new PropertyMetadata(null));

        public static readonly DependencyProperty XAxesProperty =
            DependencyProperty.Register(nameof(XAxes), typeof(IEnumerable<Axis>), typeof(LineChartCard), new PropertyMetadata(null));

        public static readonly DependencyProperty YAxesProperty =
            DependencyProperty.Register(nameof(YAxes), typeof(IEnumerable<Axis>), typeof(LineChartCard), new PropertyMetadata(null));

        public static readonly DependencyProperty ChartHeightProperty =
            DependencyProperty.Register(nameof(ChartHeight), typeof(double), typeof(LineChartCard), new PropertyMetadata(300.0));

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

        public Visibility ShowLegend
        {
            get => (Visibility)GetValue(ShowLegendProperty);
            set => SetValue(ShowLegendProperty, value);
        }

        public Visibility ShowLegend2
        {
            get => (Visibility)GetValue(ShowLegend2Property);
            set => SetValue(ShowLegend2Property, value);
        }

        public string Legend1Text
        {
            get => (string)GetValue(Legend1TextProperty);
            set => SetValue(Legend1TextProperty, value);
        }

        public string Legend2Text
        {
            get => (string)GetValue(Legend2TextProperty);
            set => SetValue(Legend2TextProperty, value);
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

        #endregion
    }
}
