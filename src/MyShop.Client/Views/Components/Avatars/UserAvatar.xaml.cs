using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MyShop.Client.Views.Components.Avatars;

/// <summary>
/// User avatar component with initials fallback and status indicator.
/// Usage:
/// <avatars:UserAvatar ImageSource="ms-appx:///Assets/user.jpg"
///                     DisplayName="John Doe"
///                     Size="Medium"
///                     Status="Online"/>
/// </summary>
public sealed partial class UserAvatar : UserControl
{
    public UserAvatar()
    {
        InitializeComponent();
        UpdateAvatarSize();
        UpdateStatusIndicator();
    }

    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(string),
            typeof(UserAvatar),
            new PropertyMetadata(null, OnImageSourceChanged));

    public string? ImageSource
    {
        get => (string?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserAvatar avatar && e.NewValue is string path && !string.IsNullOrEmpty(path))
        {
            try
            {
                avatar.AvatarImage.ProfilePicture = new BitmapImage(new Uri(path, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                // If image loading fails, fallback to initials
                avatar.AvatarImage.ProfilePicture = null;
            }
        }
    }

    public static readonly DependencyProperty DisplayNameProperty =
        DependencyProperty.Register(
            nameof(DisplayName),
            typeof(string),
            typeof(UserAvatar),
            new PropertyMetadata(string.Empty, OnDisplayNameChanged));

    public string DisplayName
    {
        get => (string)GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public static readonly DependencyProperty InitialsProperty =
        DependencyProperty.Register(
            nameof(Initials),
            typeof(string),
            typeof(UserAvatar),
            new PropertyMetadata(string.Empty));

    public string Initials
    {
        get => (string)GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(AvatarSize),
            typeof(UserAvatar),
            new PropertyMetadata(AvatarSize.Medium, OnSizeChanged));

    public AvatarSize Size
    {
        get => (AvatarSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status),
            typeof(UserStatus),
            typeof(UserAvatar),
            new PropertyMetadata(UserStatus.None, OnStatusChanged));

    public UserStatus Status
    {
        get => (UserStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public static readonly DependencyProperty IsEmailVerifiedProperty =
        DependencyProperty.Register(
            nameof(IsEmailVerified),
            typeof(bool),
            typeof(UserAvatar),
            new PropertyMetadata(false));

    public bool IsEmailVerified
    {
        get => (bool)GetValue(IsEmailVerifiedProperty);
        set => SetValue(IsEmailVerifiedProperty, value);
    }

    public static readonly DependencyProperty ShowVerificationBadgeProperty =
        DependencyProperty.Register(
            nameof(ShowVerificationBadge),
            typeof(bool),
            typeof(UserAvatar),
            new PropertyMetadata(false));

    public bool ShowVerificationBadge
    {
        get => (bool)GetValue(ShowVerificationBadgeProperty);
        set => SetValue(ShowVerificationBadgeProperty, value);
    }

    private static void OnDisplayNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserAvatar avatar && !string.IsNullOrEmpty(avatar.DisplayName))
        {
            // Generate initials from display name
            var parts = avatar.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                avatar.Initials = $"{parts[0][0]}{parts[1][0]}".ToUpper();
            }
            else if (parts.Length == 1 && parts[0].Length >= 2)
            {
                avatar.Initials = parts[0].Substring(0, 2).ToUpper();
            }
        }
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserAvatar avatar)
        {
            avatar.UpdateAvatarSize();
        }
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserAvatar avatar)
        {
            avatar.UpdateStatusIndicator();
        }
    }

    private void UpdateAvatarSize()
    {
        if (AvatarContainer == null || AvatarImage == null)
            return;

        var size = Size switch
        {
            AvatarSize.XSmall => 24.0,
            AvatarSize.Small => 32.0,
            AvatarSize.Medium => 40.0,
            AvatarSize.Large => 56.0,
            AvatarSize.XLarge => 72.0,
            _ => 40.0
        };

        AvatarContainer.Width = size;
        AvatarContainer.Height = size;
        AvatarImage.Width = size;
        AvatarImage.Height = size;
    }

    private void UpdateStatusIndicator()
    {
        if (StatusIndicator == null)
            return;

        if (Status == UserStatus.None)
        {
            StatusIndicator.Visibility = Visibility.Collapsed;
            return;
        }

        StatusIndicator.Visibility = Visibility.Visible;

        var color = Status switch
        {
            UserStatus.Online => "#10B981",   // Green
            UserStatus.Away => "#F59E0B",     // Yellow
            UserStatus.Busy => "#EF4444",     // Red
            UserStatus.Offline => "#6B7280",  // Gray
            _ => "#6B7280"
        };

        StatusIndicator.Fill = new SolidColorBrush(ColorHelper(color));
    }

    private Windows.UI.Color ColorHelper(string hex)
    {
        hex = hex.Replace("#", "");
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        return Windows.UI.Color.FromArgb(255, r, g, b);
    }
}

public enum AvatarSize
{
    XSmall,
    Small,
    Medium,
    Large,
    XLarge
}

public enum UserStatus
{
    None,
    Online,
    Away,
    Busy,
    Offline
}
