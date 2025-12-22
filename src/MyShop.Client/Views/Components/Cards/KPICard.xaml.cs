using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] START - e.NewValue: {e.NewValue} (type: {e.NewValue.GetType().Name})");

                    try
                    {
                        var oldValue = ConvertToDouble(e.OldValue);
                        System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] ✅ OldValue converted: {oldValue}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] ❌ ERROR converting OldValue: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] Stack: {ex.StackTrace}");
                    }

                    try
                    {
                        var oldValue = ConvertToDouble(e.OldValue);
                        var newValue = ConvertToDouble(e.NewValue);
                        System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] ✅ Converted - oldValue: {oldValue}, newValue: {newValue}");

                        System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] 🎬 Calling AnimateValue({oldValue}, {newValue}, 800)");
                        card.AnimateValue(oldValue, newValue, 800); // 0.8s duration
                        System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] ✅ AnimateValue completed");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] ❌ ERROR in value conversion/animation: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] Exception Type: {ex.GetType().FullName}");
                        System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] Stack: {ex.StackTrace}");

                        // Fallback: just set the display value directly
                        try
                        {
                            if (card != null)
                            {
                                var newValue = ConvertToDouble(e.NewValue);
                                System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] 🔄 Fallback: Setting DisplayValue directly to {newValue}");
                                card.DisplayValue = card.FormatValue(newValue);
                                System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] ✅ Fallback successful: DisplayValue = {card.DisplayValue}");
                            }
                        }
                        catch (Exception fallbackEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] ❌ ERROR in fallback: {fallbackEx.Message}");
                            System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] Fallback Stack: {fallbackEx.StackTrace}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] ❌ CRITICAL ERROR: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] Exception Type: {ex.GetType().FullName}");
                    System.Diagnostics.Debug.WriteLine($"[KPICard.OnValueChanged] Stack: {ex.StackTrace}");
                }
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
                var cleaned = s.Replace(",", "").Replace(" ", "").Replace("$", "").Replace(" ", "").Replace(" ", "").Trim();
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
            try
            {
                if (this.FindName("LiftUpAnimation") is Storyboard storyboard && storyboard != null)
                {
                    storyboard.Begin();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[KPICard.CardBorder_PointerEntered] Animation error: {ex.Message}");
            }
            // Note: InputSystemCursor not available in current WinUI SDK version
        }

        private void CardBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // Return to normal state
            try
            {
                if (this.FindName("LiftDownAnimation") is Storyboard storyboard && storyboard != null)
                {
                    storyboard.Begin();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[KPICard.CardBorder_PointerExited] Animation error: {ex.Message}");
            }
        }

        #endregion

        #region Animation Logic

        private void AnimateValue(double from, double to, int durationMs)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] 🎬 START - from={from}, to={to}, duration={durationMs}ms");

                // Optimization: Skip animation if no change needed
                if (Math.Abs(from - to) < 0.01)
                {
                    System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] 📌 Skipping animation (from ≈ to)");
                    try
                    {
                        var formatted = FormatValue(to);
                        DisplayValue = formatted;
                        System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ✅ Set DisplayValue directly: {formatted}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ❌ Error setting DisplayValue: {ex.Message}");
                    }
                    return;
                }

                // Simple count-up animation using DispatcherTimer
                // Note: CountUpAnimation Storyboard is removed from Begin() call to avoid WinRT exceptions
                var timer = new DispatcherTimer();
                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ✅ DispatcherTimer created");

                var startTime = DateTime.Now;
                var duration = TimeSpan.FromMilliseconds(durationMs);
                bool isDisposed = false;
                bool animationCompleted = false;
                EventHandler<object>? tickHandler = null;

                tickHandler = (s, e) =>
                {
                    if (isDisposed || animationCompleted)
                    {
                        System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] Timer tick ignored - isDisposed={isDisposed}, completed={animationCompleted}");
                        return;
                    }

                    try
                    {
                        var elapsed = DateTime.Now - startTime;
                        if (elapsed >= duration)
                        {
                            animationCompleted = true;

                            try
                            {
                                var formatted = FormatValue(to);
                                DisplayValue = formatted;
                                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ✅ Animation complete - DisplayValue = {formatted}");
                            }
                            catch (Exception setEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ⚠️ Error setting final DisplayValue: {setEx.Message}");
                                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] Stack: {setEx.StackTrace}");
                            }

                            try
                            {
                                timer.Stop();
                                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ✅ Timer stopped");
                            }
                            catch (Exception stopEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ⚠️ Error stopping timer: {stopEx.Message}");
                            }

                            try
                            {
                                if (tickHandler != null)
                                {
                                    timer.Tick -= tickHandler;
                                    System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ✅ Tick handler unsubscribed");
                                }
                            }
                            catch (Exception unsubEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ⚠️ Error unsubscribing handler: {unsubEx.Message}");
                            }

                            isDisposed = true;
                        }
                        else
                        {
                            var progress = elapsed.TotalMilliseconds / duration.TotalMilliseconds;
                            var easedProgress = EaseOutCubic(progress);
                            var currentValue = from + (to - from) * easedProgress;

                            try
                            {
                                var formatted = FormatValue(currentValue);
                                DisplayValue = formatted;
                                // Don't log every frame - too verbose
                            }
                            catch (Exception frameEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ⚠️ Error updating frame: {frameEx.Message}");
                                isDisposed = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ❌ Timer tick error: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] Exception Type: {ex.GetType().FullName}");
                        System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] Stack: {ex.StackTrace}");

                        try
                        {
                            timer?.Stop();
                            if (tickHandler != null)
                            {
                                timer?.Tick -= tickHandler;
                            }
                        }
                        catch { }

                        isDisposed = true;
                    }
                };

                timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ✅ Timer interval set to 16ms");

                timer.Tick += tickHandler;
                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ✅ Tick handler subscribed");

                timer.Start();
                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ✅ Timer started");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ❌ CRITICAL ERROR during setup: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] Exception Type: {ex.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] Stack: {ex.StackTrace}");

                System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] 🔄 Fallback: Setting DisplayValue directly");
                try
                {
                    DisplayValue = FormatValue(to);
                    System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ✅ Fallback complete - DisplayValue = {DisplayValue}");
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] ❌ Fallback also failed: {fallbackEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"[KPICard.AnimateValue] Stack: {fallbackEx.StackTrace}");
                }
            }
        }

        private string FormatValue(double value)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[FormatValue] Converting value={value}, IsCurrency={IsCurrency}");

                // Format based on IsCurrency flag
                if (IsCurrency)
                {
                    // Format large currency values (in VND) with ₫ symbol
                    // Example: 404830000 -> "404.8M₫"
                    if (value >= 1_000_000)
                    {
                        var result = $"{value / 1_000_000:F1}M₫";
                        System.Diagnostics.Debug.WriteLine($"[FormatValue] ✅ Currency (M): {value} → {result}");
                        return result;
                    }
                    else if (value >= 1_000)
                    {
                        var result = $"{value / 1_000:F1}K₫";
                        System.Diagnostics.Debug.WriteLine($"[FormatValue] ✅ Currency (K): {value} → {result}");
                        return result;
                    }
                    else
                    {
                        var result = $"{value:F0}₫";
                        System.Diagnostics.Debug.WriteLine($"[FormatValue] ✅ Currency: {value} → {result}");
                        return result;
                    }
                }
                else
                {
                    // Format as simple number with thousand separators
                    // Example: 25000 -> "25.000"
                    if (value >= 1_000_000)
                    {
                        var result = $"{value / 1_000_000:F1}M";
                        System.Diagnostics.Debug.WriteLine($"[FormatValue] ✅ Number (M): {value} → {result}");
                        return result;
                    }
                    else if (value >= 1_000)
                    {
                        var result = $"{value / 1_000:F1}K";
                        System.Diagnostics.Debug.WriteLine($"[FormatValue] ✅ Number (K): {value} → {result}");
                        return result;
                    }
                    else
                    {
                        var result = $"{value:F0}";
                        System.Diagnostics.Debug.WriteLine($"[FormatValue] ✅ Number: {value} → {result}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FormatValue] ❌ ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[FormatValue] Exception Type: {ex.GetType().FullName}");
                System.Diagnostics.Debug.WriteLine($"[FormatValue] Stack: {ex.StackTrace}");
                return "0";
            }
        }

        private double EaseOutCubic(double t)
        {
            return 1 - Math.Pow(1 - t, 3);
        }

        #endregion
    }
}
