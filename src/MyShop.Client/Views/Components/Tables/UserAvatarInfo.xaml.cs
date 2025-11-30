using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace MyShop.Client.Views.Components.Tables;

/// <summary>
/// A reusable user/agent display with avatar, name, and optional secondary text.
/// </summary>
public sealed partial class UserAvatarInfo : UserControl
{
    public UserAvatarInfo()
    {
        this.InitializeComponent();
    }

    #region DisplayName Property

    public static readonly DependencyProperty DisplayNameProperty =
        DependencyProperty.Register(
            nameof(DisplayName),
            typeof(string),
            typeof(UserAvatarInfo),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    public string DisplayName
    {
        get => (string)GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    #endregion

    #region SecondaryText Property

    public static readonly DependencyProperty SecondaryTextProperty =
        DependencyProperty.Register(
            nameof(SecondaryText),
            typeof(string),
            typeof(UserAvatarInfo),
            new PropertyMetadata(string.Empty, OnSecondaryTextChanged));

    /// <summary>
    /// Gets or sets the secondary text (e.g., email, role).
    /// </summary>
    public string SecondaryText
    {
        get => (string)GetValue(SecondaryTextProperty);
        set => SetValue(SecondaryTextProperty, value);
    }

    private static void OnSecondaryTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserAvatarInfo control)
        {
            control.HasSecondaryText = !string.IsNullOrEmpty(e.NewValue as string) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }
    }

    #endregion

    #region HasSecondaryText Property

    public static readonly DependencyProperty HasSecondaryTextProperty =
        DependencyProperty.Register(
            nameof(HasSecondaryText),
            typeof(Visibility),
            typeof(UserAvatarInfo),
            new PropertyMetadata(Visibility.Collapsed));

    /// <summary>
    /// Gets the visibility based on whether secondary text is present.
    /// </summary>
    public Visibility HasSecondaryText
    {
        get => (Visibility)GetValue(HasSecondaryTextProperty);
        private set => SetValue(HasSecondaryTextProperty, value);
    }

    #endregion

    #region ExtraContent Property

    public static readonly DependencyProperty ExtraContentProperty =
        DependencyProperty.Register(
            nameof(ExtraContent),
            typeof(object),
            typeof(UserAvatarInfo),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets extra content to display next to the name (e.g., badges).
    /// </summary>
    public object ExtraContent
    {
        get => GetValue(ExtraContentProperty);
        set => SetValue(ExtraContentProperty, value);
    }

    #endregion

    #region AvatarUrl Property

    public static readonly DependencyProperty AvatarUrlProperty =
        DependencyProperty.Register(
            nameof(AvatarUrl),
            typeof(string),
            typeof(UserAvatarInfo),
            new PropertyMetadata(string.Empty, OnAvatarUrlChanged));

    /// <summary>
    /// Gets or sets the avatar image URL.
    /// </summary>
    public string AvatarUrl
    {
        get => (string)GetValue(AvatarUrlProperty);
        set => SetValue(AvatarUrlProperty, value);
    }

    private static void OnAvatarUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserAvatarInfo control)
        {
            control.UpdateAvatarSource();
        }
    }

    #endregion

    #region AvatarSource Property (Computed)

    public static readonly DependencyProperty AvatarSourceProperty =
        DependencyProperty.Register(
            nameof(AvatarSource),
            typeof(ImageSource),
            typeof(UserAvatarInfo),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets the computed avatar image source.
    /// </summary>
    public ImageSource AvatarSource
    {
        get => (ImageSource)GetValue(AvatarSourceProperty);
        private set => SetValue(AvatarSourceProperty, value);
    }

    private void UpdateAvatarSource()
    {
        if (!string.IsNullOrEmpty(AvatarUrl))
        {
            try
            {
                AvatarSource = new BitmapImage(new Uri(AvatarUrl));
            }
            catch
            {
                AvatarSource = null;
            }
        }
        else
        {
            AvatarSource = null;
        }
    }

    #endregion

    #region Size Property

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(
            nameof(Size),
            typeof(UserAvatarSize),
            typeof(UserAvatarInfo),
            new PropertyMetadata(UserAvatarSize.Medium, OnSizeChanged));

    /// <summary>
    /// Gets or sets the avatar size (Small, Medium, Large).
    /// </summary>
    public UserAvatarSize Size
    {
        get => (UserAvatarSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UserAvatarInfo control)
        {
            control.UpdateSizeProperties();
        }
    }

    #endregion

    #region Computed Size Properties

    public static readonly DependencyProperty AvatarSizeProperty =
        DependencyProperty.Register(nameof(AvatarSize), typeof(double), typeof(UserAvatarInfo), new PropertyMetadata(40.0));

    public double AvatarSize
    {
        get => (double)GetValue(AvatarSizeProperty);
        set => SetValue(AvatarSizeProperty, value);
    }

    public static readonly DependencyProperty NameFontSizeProperty =
        DependencyProperty.Register(nameof(NameFontSize), typeof(double), typeof(UserAvatarInfo), new PropertyMetadata(14.0));

    public double NameFontSize
    {
        get => (double)GetValue(NameFontSizeProperty);
        private set => SetValue(NameFontSizeProperty, value);
    }

    public static readonly DependencyProperty SecondaryFontSizeProperty =
        DependencyProperty.Register(nameof(SecondaryFontSize), typeof(double), typeof(UserAvatarInfo), new PropertyMetadata(12.0));

    public double SecondaryFontSize
    {
        get => (double)GetValue(SecondaryFontSizeProperty);
        private set => SetValue(SecondaryFontSizeProperty, value);
    }

    private void UpdateSizeProperties()
    {
        switch (Size)
        {
            case UserAvatarSize.Small:
                AvatarSize = 32;
                NameFontSize = 13;
                SecondaryFontSize = 11;
                break;

            case UserAvatarSize.Large:
                AvatarSize = 48;
                NameFontSize = 16;
                SecondaryFontSize = 13;
                break;

            case UserAvatarSize.Medium:
            default:
                AvatarSize = 40;
                NameFontSize = 14;
                SecondaryFontSize = 12;
                break;
        }
    }

    #endregion
}

/// <summary>
/// Defines the size variants for UserAvatarInfo.
/// </summary>
public enum UserAvatarSize
{
    /// <summary>
    /// Small avatar (32px).
    /// </summary>
    Small,

    /// <summary>
    /// Medium avatar (40px) - default.
    /// </summary>
    Medium,

    /// <summary>
    /// Large avatar (48px).
    /// </summary>
    Large
}
