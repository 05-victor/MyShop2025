using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace MyShop.Client.Views.Components.Cards
{
    public sealed partial class DataTableCard : UserControl
    {
        public DataTableCard()
        {
            this.InitializeComponent();
        }

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
                typeof(DataTableCard),
                new PropertyMetadata("Data Table"));

        // TableContent Property (ContentPresenter slot)
        public object TableContent
        {
            get => GetValue(TableContentProperty);
            set => SetValue(TableContentProperty, value);
        }

        public static readonly DependencyProperty TableContentProperty =
            DependencyProperty.Register(
                nameof(TableContent),
                typeof(object),
                typeof(DataTableCard),
                new PropertyMetadata(null));

        // ShowViewAll Property
        public Visibility ShowViewAll
        {
            get => (Visibility)GetValue(ShowViewAllProperty);
            set => SetValue(ShowViewAllProperty, value);
        }

        public static readonly DependencyProperty ShowViewAllProperty =
            DependencyProperty.Register(
                nameof(ShowViewAll),
                typeof(Visibility),
                typeof(DataTableCard),
                new PropertyMetadata(Visibility.Collapsed));

        // ViewAllText Property
        public string ViewAllText
        {
            get => (string)GetValue(ViewAllTextProperty);
            set => SetValue(ViewAllTextProperty, value);
        }

        public static readonly DependencyProperty ViewAllTextProperty =
            DependencyProperty.Register(
                nameof(ViewAllText),
                typeof(string),
                typeof(DataTableCard),
                new PropertyMetadata("View All"));

        // ViewAllCommand Property
        public ICommand ViewAllCommand
        {
            get => (ICommand)GetValue(ViewAllCommandProperty);
            set => SetValue(ViewAllCommandProperty, value);
        }

        public static readonly DependencyProperty ViewAllCommandProperty =
            DependencyProperty.Register(
                nameof(ViewAllCommand),
                typeof(ICommand),
                typeof(DataTableCard),
                new PropertyMetadata(null));

        // ViewAllClicked Event
        public event RoutedEventHandler? ViewAllClicked;

        private void ViewAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Execute command if available
            if (ViewAllCommand?.CanExecute(null) == true)
            {
                ViewAllCommand.Execute(null);
            }
            
            // Also fire event for backwards compatibility
            ViewAllClicked?.Invoke(this, e);
        }
    }
}
