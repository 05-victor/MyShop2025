using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using MyShop.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyShop.Client.Views.Components.Navigation;

/// <summary>
/// Quick Actions Menu - command palette style navigation (Ctrl+K).
/// </summary>
public sealed partial class QuickActionsMenu : UserControl
{
    private readonly NavigationService? _navigationService;
    private readonly ObservableCollection<QuickAction> _allActions;
    private readonly ObservableCollection<QuickAction> _filteredActions;

    public QuickActionsMenu()
    {
        this.InitializeComponent();
        _navigationService = App.Current.Services?.GetService<NavigationService>();
        
        _allActions = new ObservableCollection<QuickAction>(GetDefaultActions());
        _filteredActions = new ObservableCollection<QuickAction>(_allActions);
        ActionsList.ItemsSource = _filteredActions;
    }

    /// <summary>
    /// Shows the quick actions menu.
    /// </summary>
    public void Show()
    {
        OverlayGrid.Visibility = Visibility.Visible;
        (Resources["FadeInStoryboard"] as Storyboard)?.Begin();
        SearchBox.Text = string.Empty;
        FilterActions(string.Empty);
        SearchBox.Focus(FocusState.Programmatic);
    }

    /// <summary>
    /// Hides the quick actions menu.
    /// </summary>
    public void Hide()
    {
        var storyboard = Resources["FadeOutStoryboard"] as Storyboard;
        if (storyboard != null)
        {
            storyboard.Completed += (s, e) => OverlayGrid.Visibility = Visibility.Collapsed;
            storyboard.Begin();
        }
        else
        {
            OverlayGrid.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Toggles visibility.
    /// </summary>
    public void Toggle()
    {
        if (OverlayGrid.Visibility == Visibility.Visible)
            Hide();
        else
            Show();
    }

    private void CloseOverlay_Click(object sender, RoutedEventArgs e) => Hide();

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            FilterActions(sender.Text);
        }
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (_filteredActions.Count > 0)
        {
            ExecuteAction(_filteredActions[0]);
        }
    }

    private void ActionsList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is QuickAction action)
        {
            ExecuteAction(action);
        }
    }

    private void FilterActions(string query)
    {
        _filteredActions.Clear();
        
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allActions
            : _allActions.Where(a => 
                a.Title.Contains(query, System.StringComparison.OrdinalIgnoreCase) ||
                (a.Keywords?.Any(k => k.Contains(query, System.StringComparison.OrdinalIgnoreCase)) ?? false));

        foreach (var action in filtered)
        {
            _filteredActions.Add(action);
        }
    }

    private void ExecuteAction(QuickAction action)
    {
        Hide();
        
        if (!string.IsNullOrEmpty(action.PageName))
        {
            _navigationService?.NavigateInShell(action.PageName, action.Parameter);
        }
        
        action.Action?.Invoke();
    }

    private static QuickAction[] GetDefaultActions() => new[]
    {
        new QuickAction { Title = "Dashboard", PageName = "AdminDashboardPage", IconGlyph = "\uE80F", Shortcut = "", Keywords = new[] { "home", "main" } },
        new QuickAction { Title = "Products", PageName = "AdminProductsPage", IconGlyph = "\uE7BF", Keywords = new[] { "inventory", "items" } },
        new QuickAction { Title = "Orders", PageName = "AdminOrdersPage", IconGlyph = "\uE8A7", Keywords = new[] { "sales", "transactions" } },
        new QuickAction { Title = "Users", PageName = "AdminUsersPage", IconGlyph = "\uE77B", Keywords = new[] { "customers", "accounts" } },
        new QuickAction { Title = "Reports", PageName = "AdminReportsPage", IconGlyph = "\uE9F9", Keywords = new[] { "analytics", "statistics" } },
        new QuickAction { Title = "Settings", PageName = "SettingsPage", IconGlyph = "\uE713", Keywords = new[] { "config", "preferences" } },
        new QuickAction { Title = "Profile", PageName = "ProfilePage", IconGlyph = "\uE77B", Keywords = new[] { "account", "user" } },
    };
}

/// <summary>
/// Represents a quick action item.
/// </summary>
public class QuickAction
{
    public string Title { get; set; } = string.Empty;
    public string PageName { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "\uE80F";
    public string? Shortcut { get; set; }
    public string[]? Keywords { get; set; }
    public object? Parameter { get; set; }
    public System.Action? Action { get; set; }
}
