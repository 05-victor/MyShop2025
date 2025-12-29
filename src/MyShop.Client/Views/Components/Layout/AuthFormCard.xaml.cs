using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components.Layout
{
    /// <summary>
    /// AuthFormCard is a reusable card component for auth form content.
    /// </summary>
    public sealed partial class AuthFormCard : UserControl
    {
        public AuthFormCard()
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
                typeof(AuthFormCard),
                new PropertyMetadata(null));
    }
}
