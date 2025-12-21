using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;
using System.Globalization;

namespace MyShop.Client.Views.Components.Cards
{
    public sealed partial class KPICard : UserControl
    {
        public KPICard()
        {
            this.InitializeComponent();
            this.Loaded += KPICard_Loaded;
        }

        #region Dependency Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(KPICard), new PropertyMetadata("KPI Title"));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(string), typeof(KPICard), new PropertyMetadata("\uE7B8")); // Default: Box icon

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(KPICard), new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty DisplayValueProperty =
            DependencyProperty.Register(nameof(DisplayValue), typeof(string), typeof(KPICard), new PropertyMetadata("0"));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(KPICard), new PropertyMetadata("Description"));

        public static readonly DependencyProperty IconBackgroundBrushProperty =
            DependencyProperty.Register(nameof(IconBackgroundBrush), typeof(Brush), typeof(KPICard),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 190, 239, 255)))); // #BEEFFF

        public static readonly DependencyProperty IconForegroundBrushProperty =
            DependencyProperty.Register(nameof(IconForegroundBrush), typeof(Brush), typeof(KPICard),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 0, 174, 239)))); // #00AEEF

        public static readonly DependencyProperty ShowBadgeProperty =
            DependencyProperty.Register(nameof(ShowBadge), typeof(Visibility), typeof(KPICard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty TrendIconProperty =
            DependencyProperty.Register(nameof(TrendIcon), typeof(string), typeof(KPICard), new PropertyMetadata("\uE74E")); // Up arrow

        public static readonly DependencyProperty TrendTextProperty =
            DependencyProperty.Register(nameof(TrendText), typeof(string), typeof(KPICard), new PropertyMetadata("+0%"));

        public static readonly DependencyProperty BadgeBackgroundBrushProperty =
            DependencyProperty.Register(nameof(BadgeBackgroundBrush), typeof(Brush), typeof(KPICard),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 220, 252, 231)))); // Green light

        public static readonly DependencyProperty BadgeForegroundBrushProperty =
            DependencyProperty.Register(nameof(BadgeForegroundBrush), typeof(Brush), typeof(KPICard),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 16, 185, 129)))); // #10B981

        public static readonly DependencyProperty IsCurrencyProperty =
            DependencyProperty.Register(nameof(IsCurrency), typeof(bool), typeof(KPICard), new PropertyMetadata(false));

        #endregion

        #region Properties

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string DisplayValue
        {
            get => (string)GetValue(DisplayValueProperty);
            set => SetValue(DisplayValueProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public Brush IconBackgroundBrush
        {
            get => (Brush)GetValue(IconBackgroundBrushProperty);
            set => SetValue(IconBackgroundBrushProperty, value);
        }

        public Brush IconForegroundBrush
        {
            get => (Brush)GetValue(IconForegroundBrushProperty);
            set => SetValue(IconForegroundBrushProperty, value);
        }

        public Visibility ShowBadge
        {
            get => (Visibility)GetValue(ShowBadgeProperty);
            set => SetValue(ShowBadgeProperty, value);
        }

        public string TrendIcon
        {
            get => (string)GetValue(TrendIconProperty);
            set => SetValue(TrendIconProperty, value);
        }

        public string TrendText
        {
            get => (string)GetValue(TrendTextProperty);
            set => SetValue(TrendTextProperty, value);
        }

        public Brush BadgeBackgroundBrush
        {
            get => (Brush)GetValue(BadgeBackgroundBrushProperty);
            set => SetValue(BadgeBackgroundBrushProperty, value);
        }

        public Brush BadgeForegroundBrush
        {
            get => (Brush)GetValue(BadgeForegroundBrushProperty);
            set => SetValue(BadgeForegroundBrushProperty, value);
        }

        public bool IsCurrency
        {
            get => (bool)GetValue(IsCurrencyProperty);
            set => SetValue(IsCurrencyProperty, value);
        }

        #endregion

        #region Event Handlers

        private void KPICard_Loaded(object sender, RoutedEventArgs e)
        {
            // Trigger count-up animation on load
            var doubleValue = ConvertToDouble(Value);
            AnimateValue(0, doubleValue, 1000); // 1 second duration
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is KPICard card && e.NewValue != null)
            {
                System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] e.NewValue: {e.NewValue} (type: {e.NewValue.GetType().Name})");
                var oldValue = ConvertToDouble(e.OldValue);
                var newValue = ConvertToDouble(e.NewValue);
                System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] Converted - oldValue: {oldValue}, newValue: {newValue}");
                card.AnimateValue(oldValue, newValue, 800); // 0.8s duration
            }
        }

        private static double ConvertToDouble(object value)
        {
            if (value == null) return 0.0;
            if (value is double d) return d;
            if (value is decimal dec) return (double)dec;
            if (value is int i) return i;
            if (value is long l) return l;
            if (value is float f) return f;

            if (value is string s)
            {
                // Try parsing as currency/number using current culture (decimal for money accuracy)
                if (decimal.TryParse(s, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out var decParsed))
                    return (double)decParsed;

                // Fallback: remove common grouping and currency symbols then try invariant parse
                var cleaned = s.Replace(",", "").Replace(" ", "").Replace("$", "").Replace("�", "").Replace("�", "").Trim();
                if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out decParsed))
                    return (double)decParsed;

                // Last resort: try double parsing with current culture
                if (double.TryParse(s, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, CultureInfo.CurrentCulture, out var dParsed))
                    return dParsed;

                return 0.0;
            }

            try
            {
                return Convert.ToDouble(value, CultureInfo.CurrentCulture);
            }
            catch
            {
                return 0.0;
            }
        }

        private void CardBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // WinUI Gallery pattern: Lift card up with shadow expansion
            LiftUpAnimation.Begin();
            // Note: InputSystemCursor not available in current WinUI SDK version
        }

        private void CardBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Return to normal state
            LiftDownAnimation.Begin();
        }

        #endregion

        #region Animation Logic

        private void AnimateValue(double from, double to, int durationMs)
        {
            // Simple count-up animation using DispatcherTimer
            var timer = new DispatcherTimer();
            var startTime = DateTime.Now;
            var duration = TimeSpan.FromMilliseconds(durationMs);

            timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            timer.Tick += (s, e) =>
            {
                var elapsed = DateTime.Now - startTime;
                if (elapsed >= duration)
                {
                    DisplayValue = FormatValue(to);
                    timer.Stop();
                    CountUpAnimation.Begin(); // Fade-in effect
                }
                else
                {
                    var progress = elapsed.TotalMilliseconds / duration.TotalMilliseconds;
                    var easedProgress = EaseOutCubic(progress);
                    var currentValue = from + (to - from) * easedProgress;
                    DisplayValue = FormatValue(currentValue);
                }
            };

            timer.Start();
        }

        private string FormatValue(double value)
        {
            // Format based on IsCurrency flag
            if (IsCurrency)
            {
                // Format large currency values (in VND) with ₫ symbol
                // Example: 404830000 -> "404.8M₫"
                if (value >= 1_000_000)
                    return $"{value / 1_000_000:F1}M₫";
                else if (value >= 1_000)
                    return $"{value / 1_000:F1}K₫";
                else
                    return $"{value:F0}₫";
            }
            else
            {
                // Format as simple number with thousand separators
                // Example: 25000 -> "25.000"
                if (value >= 1_000_000)
                    return $"{value / 1_000_000:F1}M";
                else if (value >= 1_000)
                    return $"{value / 1_000:F1}K";
                else
                    return $"{value:F0}";
            }
        }

        private double EaseOutCubic(double t)
        {
            return 1 - Math.Pow(1 - t, 3);
        }

        #endregion
    }
}
