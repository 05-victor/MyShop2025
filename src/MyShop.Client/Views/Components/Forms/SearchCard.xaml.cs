using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections;
using Windows.Foundation;

namespace MyShop.Client.Views.Components.Forms;

/// <summary>
/// A reusable search card with AutoSuggestBox and optional search button.
/// </summary>
public sealed partial class SearchCard : UserControl
{
    public SearchCard()
    {
        this.InitializeComponent();
    }

    #region Title Property

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SearchCard),
            new PropertyMetadata(string.Empty, OnTitleChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SearchCard card)
        {
            card.HasTitle = !string.IsNullOrEmpty(e.NewValue as string) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }
    }

    #endregion

    #region HasTitle Property

    public static readonly DependencyProperty HasTitleProperty =
        DependencyProperty.Register(nameof(HasTitle), typeof(Visibility), typeof(SearchCard), 
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility HasTitle
    {
        get => (Visibility)GetValue(HasTitleProperty);
        private set => SetValue(HasTitleProperty, value);
    }

    #endregion

    #region Subtitle Property

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(SearchCard),
            new PropertyMetadata(string.Empty, OnSubtitleChanged));

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SearchCard card)
        {
            card.HasSubtitle = !string.IsNullOrEmpty(e.NewValue as string) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }
    }

    #endregion

    #region HasSubtitle Property

    public static readonly DependencyProperty HasSubtitleProperty =
        DependencyProperty.Register(nameof(HasSubtitle), typeof(Visibility), typeof(SearchCard), 
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility HasSubtitle
    {
        get => (Visibility)GetValue(HasSubtitleProperty);
        private set => SetValue(HasSubtitleProperty, value);
    }

    #endregion

    #region PlaceholderText Property

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(SearchCard),
            new PropertyMetadata("Search..."));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    #endregion

    #region SearchText Property

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(
            nameof(SearchText),
            typeof(string),
            typeof(SearchCard),
            new PropertyMetadata(string.Empty));

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    #endregion

    #region ItemsSource Property

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(SearchCard),
            new PropertyMetadata(null));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    #endregion

    #region ShowSearchButton Property

    public static readonly DependencyProperty ShowSearchButtonProperty =
        DependencyProperty.Register(
            nameof(ShowSearchButton),
            typeof(Visibility),
            typeof(SearchCard),
            new PropertyMetadata(Visibility.Visible));

    public Visibility ShowSearchButton
    {
        get => (Visibility)GetValue(ShowSearchButtonProperty);
        set => SetValue(ShowSearchButtonProperty, value);
    }

    #endregion

    #region Events

    public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxTextChangedEventArgs> TextChanged;
    public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;
    public event TypedEventHandler<AutoSuggestBox, AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;
    public event RoutedEventHandler SearchRequested;

    private void SearchAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        TextChanged?.Invoke(sender, args);
    }

    private void SearchAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        SuggestionChosen?.Invoke(sender, args);
    }

    private void SearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        QuerySubmitted?.Invoke(sender, args);
        SearchRequested?.Invoke(this, new RoutedEventArgs());
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        SearchRequested?.Invoke(this, e);
    }

    #endregion
}
