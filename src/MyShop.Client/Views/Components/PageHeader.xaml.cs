using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components
{
    public sealed partial class PageHeader : UserControl
    {
        public PageHeader()
        {
            this.InitializeComponent();
        }

        #region Dependency Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(PageHeader),
                new PropertyMetadata("Page Title"));

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(
                nameof(Subtitle),
                typeof(string),
                typeof(PageHeader),
                new PropertyMetadata(string.Empty, OnSubtitleChanged));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(string),
                typeof(PageHeader),
                new PropertyMetadata(string.Empty, OnIconChanged));

        private static readonly DependencyProperty SubtitleVisibilityProperty =
            DependencyProperty.Register(
                nameof(SubtitleVisibility),
                typeof(Visibility),
                typeof(PageHeader),
                new PropertyMetadata(Visibility.Collapsed));

        private static readonly DependencyProperty IconVisibilityProperty =
            DependencyProperty.Register(
                nameof(IconVisibility),
                typeof(Visibility),
                typeof(PageHeader),
                new PropertyMetadata(Visibility.Collapsed));

        #endregion

        #region Public Properties

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

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        private Visibility SubtitleVisibility
        {
            get => (Visibility)GetValue(SubtitleVisibilityProperty);
            set => SetValue(SubtitleVisibilityProperty, value);
        }

        private Visibility IconVisibility
        {
            get => (Visibility)GetValue(IconVisibilityProperty);
            set => SetValue(IconVisibilityProperty, value);
        }

        #endregion

        #region Property Changed Handlers

        private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageHeader control)
            {
                control.SubtitleVisibility = string.IsNullOrWhiteSpace(e.NewValue as string)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageHeader control)
            {
                control.IconVisibility = string.IsNullOrWhiteSpace(e.NewValue as string)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        #endregion
    }
}
