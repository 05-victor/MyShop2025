using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components.Layout
{
    /// <summary>
    /// Reusable authentication layout component providing 2-column grid structure
    /// with branding panel (left) and form panel (right).
    /// </summary>
    public sealed partial class AuthLayout : UserControl
    {
        public AuthLayout()
        {
            this.InitializeComponent();
        }

        public UIElement FormContent
        {
            get { return (UIElement)GetValue(FormContentProperty); }
            set { SetValue(FormContentProperty, value); }
        }

        public static readonly DependencyProperty FormContentProperty =
            DependencyProperty.Register(
                nameof(FormContent),
                typeof(UIElement),
                typeof(AuthLayout),
                new PropertyMetadata(null));
    }
}
