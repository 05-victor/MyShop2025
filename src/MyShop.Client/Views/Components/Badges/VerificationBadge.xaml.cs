using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components.Badges
{
    public sealed partial class VerificationBadge : UserControl
    {
        public static readonly DependencyProperty IsVerifiedProperty =
            DependencyProperty.Register(
                nameof(IsVerified),
                typeof(bool),
                typeof(VerificationBadge),
                new PropertyMetadata(false, OnIsVerifiedChanged));

        public bool IsVerified
        {
            get => (bool)GetValue(IsVerifiedProperty);
            set => SetValue(IsVerifiedProperty, value);
        }

        public VerificationBadge()
        {
            this.InitializeComponent();
            this.Loaded += VerificationBadge_Loaded;
        }

        private void VerificationBadge_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBadgeVisibility();
        }

        private static void OnIsVerifiedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VerificationBadge badge)
            {
                badge.UpdateBadgeVisibility();
            }
        }

        private void UpdateBadgeVisibility()
        {
            if (IsVerified)
            {
                // Show verified badge
                VerifiedBadge.Visibility = Visibility.Visible;
                UnverifiedBadge.Visibility = Visibility.Collapsed;
                
                // Stop pulse animation
                PulseAnimation.Stop();
            }
            else
            {
                // Show unverified badge with pulse
                VerifiedBadge.Visibility = Visibility.Collapsed;
                UnverifiedBadge.Visibility = Visibility.Visible;
                
                // Start pulse animation
                PulseAnimation.Begin();
            }
        }
    }
}
