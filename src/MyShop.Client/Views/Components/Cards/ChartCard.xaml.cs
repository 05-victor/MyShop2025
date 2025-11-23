using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace MyShop.Client.Views.Components.Cards
{
    public sealed partial class ChartCard : UserControl
    {
        public ChartCard()
        {
            this.InitializeComponent();
        }

        #region Dependency Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ChartCard), new PropertyMetadata("Chart Title"));

        public static readonly DependencyProperty ShowMenuProperty =
            DependencyProperty.Register(nameof(ShowMenu), typeof(Visibility), typeof(ChartCard), new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ShowActionsProperty =
            DependencyProperty.Register(nameof(ShowActions), typeof(Visibility), typeof(ChartCard), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register(nameof(Actions), typeof(object), typeof(ChartCard), new PropertyMetadata(null));

        public static readonly DependencyProperty ChartContentProperty =
            DependencyProperty.Register(nameof(ChartContent), typeof(object), typeof(ChartCard), new PropertyMetadata(null));

        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register(nameof(MenuItems), typeof(ObservableCollection<MenuFlyoutItemBase>), typeof(ChartCard), 
                new PropertyMetadata(null, OnMenuItemsChanged));

        #endregion

        #region Properties

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Visibility ShowMenu
        {
            get => (Visibility)GetValue(ShowMenuProperty);
            set => SetValue(ShowMenuProperty, value);
        }

        public Visibility ShowActions
        {
            get => (Visibility)GetValue(ShowActionsProperty);
            set => SetValue(ShowActionsProperty, value);
        }

        public object Actions
        {
            get => GetValue(ActionsProperty);
            set => SetValue(ActionsProperty, value);
        }

        public object ChartContent
        {
            get => GetValue(ChartContentProperty);
            set => SetValue(ChartContentProperty, value);
        }

        public ObservableCollection<MenuFlyoutItemBase> MenuItems
        {
            get => (ObservableCollection<MenuFlyoutItemBase>)GetValue(MenuItemsProperty);
            set => SetValue(MenuItemsProperty, value);
        }

        #endregion

        #region Event Handlers

        private static void OnMenuItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChartCard card && e.NewValue is ObservableCollection<MenuFlyoutItemBase> items)
            {
                card.ChartMenuFlyout.Items.Clear();
                foreach (var item in items)
                {
                    card.ChartMenuFlyout.Items.Add(item);
                }
            }
        }

        #endregion
    }
}
