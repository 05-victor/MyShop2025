using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components
{
    /// <summary>
    /// Reusable InfoBar component for displaying error, warning, success, and info messages
    /// </summary>
    public sealed partial class ErrorInfoBar : UserControl
    {
        public ErrorInfoBar()
        {
            this.InitializeComponent();
        }

        // IsOpen Property
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(ErrorInfoBar),
                new PropertyMetadata(false));

        // Severity Property
        public InfoBarSeverity Severity
        {
            get => (InfoBarSeverity)GetValue(SeverityProperty);
            set => SetValue(SeverityProperty, value);
        }

        public static readonly DependencyProperty SeverityProperty =
            DependencyProperty.Register(
                nameof(Severity),
                typeof(InfoBarSeverity),
                typeof(ErrorInfoBar),
                new PropertyMetadata(InfoBarSeverity.Error, OnSeverityChanged));

        // Title Property
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(ErrorInfoBar),
                new PropertyMetadata(string.Empty));

        // Message Property
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(ErrorInfoBar),
                new PropertyMetadata(string.Empty));

        // ShowIcon Property
        public bool ShowIcon
        {
            get => (bool)GetValue(ShowIconProperty);
            set => SetValue(ShowIconProperty, value);
        }

        public static readonly DependencyProperty ShowIconProperty =
            DependencyProperty.Register(
                nameof(ShowIcon),
                typeof(bool),
                typeof(ErrorInfoBar),
                new PropertyMetadata(true));

        // IsClosable Property
        public bool IsClosable
        {
            get => (bool)GetValue(IsClosableProperty);
            set => SetValue(IsClosableProperty, value);
        }

        public static readonly DependencyProperty IsClosableProperty =
            DependencyProperty.Register(
                nameof(IsClosable),
                typeof(bool),
                typeof(ErrorInfoBar),
                new PropertyMetadata(true));

        // IconGlyph Property
        public string IconGlyph
        {
            get => (string)GetValue(IconGlyphProperty);
            set => SetValue(IconGlyphProperty, value);
        }

        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register(
                nameof(IconGlyph),
                typeof(string),
                typeof(ErrorInfoBar),
                new PropertyMetadata("\uE783")); // Default error icon

        private static void OnSeverityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ErrorInfoBar control && e.NewValue is InfoBarSeverity severity)
            {
                // Update icon based on severity
                control.IconGlyph = severity switch
                {
                    InfoBarSeverity.Error => "\uE783",      // ErrorBadge
                    InfoBarSeverity.Warning => "\uE7BA",    // Warning
                    InfoBarSeverity.Success => "\uE73E",    // CheckMark
                    InfoBarSeverity.Informational => "\uE946", // Info
                    _ => "\uE783"
                };

                // Update title if not set
                if (string.IsNullOrEmpty(control.Title))
                {
                    control.Title = severity switch
                    {
                        InfoBarSeverity.Error => "Error",
                        InfoBarSeverity.Warning => "Warning",
                        InfoBarSeverity.Success => "Success",
                        InfoBarSeverity.Informational => "Information",
                        _ => string.Empty
                    };
                }
            }
        }

        private void InfoBarControl_Closed(InfoBar sender, InfoBarClosedEventArgs args)
        {
            IsOpen = false;
        }

        /// <summary>
        /// Show error message with optional title
        /// </summary>
        public void ShowError(string message, string? title = null)
        {
            Severity = InfoBarSeverity.Error;
            Message = message;
            Title = title ?? "Error";
            IsOpen = true;
        }

        /// <summary>
        /// Show warning message with optional title
        /// </summary>
        public void ShowWarning(string message, string? title = null)
        {
            Severity = InfoBarSeverity.Warning;
            Message = message;
            Title = title ?? "Warning";
            IsOpen = true;
        }

        /// <summary>
        /// Show success message with optional title
        /// </summary>
        public void ShowSuccess(string message, string? title = null)
        {
            Severity = InfoBarSeverity.Success;
            Message = message;
            Title = title ?? "Success";
            IsOpen = true;
        }

        /// <summary>
        /// Show info message with optional title
        /// </summary>
        public void ShowInfo(string message, string? title = null)
        {
            Severity = InfoBarSeverity.Informational;
            Message = message;
            Title = title ?? "Information";
            IsOpen = true;
        }

        /// <summary>
        /// Close the InfoBar
        /// </summary>
        public void Close()
        {
            IsOpen = false;
        }
    }
}
