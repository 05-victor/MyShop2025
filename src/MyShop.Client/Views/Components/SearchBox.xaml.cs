using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Client.Views.Components;

/// <summary>
/// Reusable SearchBox component with clear button.
/// Features:
/// - Search icon on left
/// - Clear button appears when text is entered
/// - Two-way binding for Text property
/// </summary>
public sealed partial class SearchBox : UserControl
{
    public SearchBox()
    {
        this.InitializeComponent();
    }

    #region Dependency Properties

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(SearchBox),
            new PropertyMetadata(string.Empty, OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(SearchBox),
            new PropertyMetadata("Search..."));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly DependencyProperty HasTextProperty =
        DependencyProperty.Register(
            nameof(HasText),
            typeof(Visibility),
            typeof(SearchBox),
            new PropertyMetadata(Visibility.Collapsed));

    public Visibility HasText
    {
        get => (Visibility)GetValue(HasTextProperty);
        private set => SetValue(HasTextProperty, value);
    }

    #endregion

    #region Event Handlers

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SearchBox searchBox)
        {
            var newText = e.NewValue as string;
            searchBox.HasText = string.IsNullOrEmpty(newText) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        Text = string.Empty;
        SearchTextBox.Focus(FocusState.Programmatic);
    }

    #endregion
}
